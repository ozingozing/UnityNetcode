using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridGizmo : MonoBehaviour
{
	public static GridGizmo instance;

	public bool displayGridGizmos;
	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public int blurSize = 3;
	public float hexRadius; // �������� Ÿ�� ������
	public TerrainType[] walkableRegions;
	public LayerMask walkableMask;
	public Dictionary<int, int> walkableRegionDictionary = new Dictionary<int, int>();
	Node[,] grid;

	[SerializeField]private int obstacleProximityPenalty;
	float hexWidth, hexHeight, hexHorizontalSpacing, hexVerticalSpacing;
	int gridSizeX, gridSizeY;
	[SerializeField] int penaltyMin = 0;
	[SerializeField] int penaltyMax = 0;
	bool isGridReady = false;

	public int MaxSize
	{
		get { return gridSizeX * gridSizeY; }
	}

	void Awake()
	{
		instance = this;
		// �������� Ÿ�� ũ�� ���
		hexWidth = Mathf.Sqrt(3) * hexRadius; // ���� �ʺ�
		hexHeight = 2 * hexRadius; // ���� ����
		hexHorizontalSpacing = hexWidth; // ���� ����: �鳢�� �ٵ��� ����
		hexVerticalSpacing = hexHeight * 0.75f; // ���� ����: �鳢�� �ٵ��� ����

		// �׸��� ũ�� ���
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / hexHorizontalSpacing);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / hexVerticalSpacing);

		foreach (TerrainType region in walkableRegions)
		{
			walkableMask.value |= region.terrainLayerMask;
			walkableRegionDictionary.Add((int)Mathf.Log(region.terrainLayerMask.value, 2), region.terrainPenalty);
		}

		penaltyMax = walkableRegionDictionary.Values.Max();
		penaltyMin = walkableRegionDictionary.Values.Min();

		/*Debug.Log("Starting Grid Generation...");
		isGridReady = await CreateGrid(blurSize);
		Debug.Log("Grid Generation Completed!");*/
	}

	public async Task DoCreateGrid()
	{
		isGridReady = false;
		Debug.Log("Starting Grid Generation...");
		isGridReady = await CreateGrid(blurSize);
		Debug.Log("Grid Generation Completed!");
	}
	
	async Task<bool> CreateGrid(int blurSize)
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int r = 0; r < gridSizeY; r++)
		{
			for (int q = 0; q < gridSizeX; q++)
			{
				// Ȧ�� �࿡ xOffset �߰��Ͽ� �¹����� ��ġ
				float xOffset = (r % 2 == 1) ? hexWidth * 0.5f : 0;

				// �������� �߽� ��ǥ ���
				Vector3 worldPoint = worldBottomLeft +
									 new Vector3(
										 q * hexHorizontalSpacing + xOffset,
										 0,
										 r * hexVerticalSpacing);

				bool walkable = !Physics.CheckSphere(worldPoint, hexRadius, unwalkableMask);

				int movementPenalty = 0;
				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				if (Physics.Raycast(ray, out RaycastHit hit, 100, walkableMask))
				{
					walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
				}

				if (!walkable)
				{
					movementPenalty += obstacleProximityPenalty;
				}

				grid[q, r] = new Node(walkable, worldPoint, q, r, movementPenalty);
			}
		}
		
		return await BlurPenaltyMap(blurSize);
	}

	public GameObject Check;
	public Node CheckAgain(Vector3 pos)
	{
		Node centerNode = NodeFromWorldPoint(pos);
		List<Node> nodes = GetNeighbours(centerNode, true);
		nodes.Add(centerNode);

		foreach (Node item in nodes)
		{
			bool walkable = !Physics.CheckSphere(item.worldPosition + Vector3.up * 2, hexRadius * 1.1f, unwalkableMask);
			//Instantiate(Check, item.worldPosition, Quaternion.identity);
			int movementPenalty = 0;
			Ray ray = new Ray(item.worldPosition + Vector3.up * 50, Vector3.down);
			if (Physics.Raycast(ray, out RaycastHit hit, 100, walkableMask))
			{
				walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
			}

			if (!walkable)
			{
				movementPenalty += obstacleProximityPenalty;
			}

			item.ReSetWalkable(walkable, false);
			item.ReSetMovementPenalty(movementPenalty * 2, false);
		}

		return ApplyAgainLocalBlur(centerNode, nodes);
	}

	//����� ������ �� ��밡 ��ó���ϰų� UnWalkableó���� ��嵵 ���� ���½�Ŵ �̰� ���ƾ���
	Node ApplyAgainLocalBlur(Node centerNode, List<Node> nodes)
	{
		// ���� ���� �� �̿� ��� Ž��
		foreach (Node item in nodes)
		{
			// ��ȿ�� ������� Ȯ��
			Node currentNode = item;
			int totalPenalty = 0;
			int totalCount = 0;

			foreach (Node item2 in GetNeighbours(currentNode))
			{
				totalPenalty += item2.movementPenalty;
				totalCount++;
			}

			// ��հ� ���
			int blurredPenalty = Mathf.RoundToInt((float)totalPenalty / totalCount);
			currentNode.ReSetMovementPenalty(blurredPenalty, false);
			currentNode.Owner = centerNode;

			centerNode.SetpenaltiesTemp(currentNode);
		}
		return centerNode;
	}

	public List<Node> GetNeighbours(Node node, bool oneMore = false)
	{
		List<Node> neighbours = new List<Node>();
		//int[] dq = { 1,-1,  0,0,  1,-1 };
		//int[] dr = { 0,0,  1,-1,  -1,1 };

		int[] dq_even = { 1,-1,  1, 0,  1, 0 };
		int[] dq_odd =  { 1,-1,  0,-1,  0,-1 };
		int[] dr =	    { 0, 0,  1,-1, -1, 1 };

		int[] dq_even2 = { 2,-2,  0, 0,  2, 1,  -1,-1,  1, 2,  -1,-1};
		int[] dq_odd2  = { 2,-2,  0, 0,  1, 1,  -2,-1,  1, 1,  -2,-1};
		int[] dr2      = { 0, 0, -2, 2,  1, 2,  -1,-2, -2,-1,   1, 2};

		bool isOddRow = node.gridY % 2 == 0;

		// ������ ���� Ž��
		for (int i = 0; i < 6; i++) // 6���� Ž��
		{
			int checkQ = node.gridX + (isOddRow ? dq_odd[i] : dq_even[i]);
			int checkR = node.gridY + dr[i];

			// ��ȿ�� ���� �� ��常 �߰�
			if (checkQ >= 0 && checkQ < gridSizeX && checkR >= 0 && checkR < gridSizeY)
			{
				neighbours.Add(grid[checkQ, checkR]);
			}
		}

		if(oneMore)
		{
			for(int i = 0; i < 12; i++)
			{
				int checkQ = node.gridX + (isOddRow ? dq_odd2[i] : dq_even2[i]);
				int checkR = node.gridY + dr2[i];

				// ��ȿ�� ���� �� ��常 �߰�
				if (checkQ >= 0 && checkQ < gridSizeX && checkR >= 0 && checkR < gridSizeY)
				{
					neighbours.Add(grid[checkQ, checkR]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		//TotalSzie of Center (0, 0) == LeftButtom + HalfSize
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int r = Mathf.Clamp(Mathf.RoundToInt((gridSizeY) * percentY), 0, gridSizeY - 1);
		int q = Mathf.Clamp(Mathf.RoundToInt((gridSizeX) * percentX), 0, gridSizeX - 1);

		return grid[q, r];
	}

	async Task<bool> BlurPenaltyMap(int blurSize)
	{
		return await Task.Run(() =>
		{
			try
			{
				if(blurSize > 0)
				{
					int kernelSize = blurSize * 2 + 1;
					int kernelExtents = (kernelSize - 1) / 2;

					// �ӽ� �迭 ����
					int[,] penaltiesTemp = new int[gridSizeX, gridSizeY];

					// �� ����� ���� �� ���
					for (int r = 0; r < gridSizeY; r++)
					{
						for (int q = 0; q < gridSizeX; q++)
						{
							int totalPenalty = 0;
							int totalCount = 0;

							// ���� ���� �� �̿� ��� Ž��
							for (int dr = -kernelExtents; dr <= kernelExtents; dr++)
							{
								for (int dq = -kernelExtents; dq <= kernelExtents; dq++)
								{
									// Ȧ�� �� ����
									int adjustedQ = q + dq + ((r % 2 == 1 && dr % 2 == 0) || (r % 2 != 1 && dr % 2 != 0) ? 1 : 0);
									int adjustedR = r + dr;

									// ��ȿ�� ������� Ȯ��
									if (adjustedQ >= 0 && adjustedQ < gridSizeX && adjustedR >= 0 && adjustedR < gridSizeY)
									{
										totalPenalty += grid[adjustedQ, adjustedR].movementPenalty;
										totalCount++;
									}
								}
							}

							// ��հ��� �ӽ� �迭�� ����
							penaltiesTemp[q, r] = Mathf.RoundToInt((float)totalPenalty / totalCount);
						}
					}

					// ������ ���� ���� �׸��忡 �ݿ�
					for (int r = 0; r < gridSizeY; r++)
					{
						for (int q = 0; q < gridSizeX; q++)
						{
							grid[q, r].ReSetMovementPenalty(penaltiesTemp[q, r], true);

							// �ִ�/�ּ� �г�Ƽ �� ������Ʈ
							penaltyMax = Mathf.Max(penaltyMax, penaltiesTemp[q, r]);
							penaltyMin = Mathf.Min(penaltyMin, penaltiesTemp[q, r]);
						}
					}
				}

				return true; // �۾� ���������� �Ϸ�
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"BlurPenaltyMap Error: {ex.Message}");
				return false; // �۾� ����
			}
		});
	}

	private void OnDrawGizmos()
	{
		if(isGridReady)
		{
			Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

			if (grid != null && displayGridGizmos)
			{
				foreach (Node n in grid)
				{
					Gizmos.color = Color.Lerp(Color.green, Color.blue, Mathf.InverseLerp(penaltyMin, penaltyMax / 2, n.movementPenalty));
					// �������� ���� ���� (walkable ���ο� ����)
					Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;

					DrawHexagon(n.worldPosition, hexRadius);
				}
			}
		}
	}

	// ���������� �׸��� �޼���
	private void DrawHexagon(Vector3 center, float radius)
	{
		Vector3[] vertices = new Vector3[6];
		float rotationOffset = Mathf.Deg2Rad * -30; // -30�Ƹ� �������� ��ȯ

		for (int i = 0; i < 6; i++)
		{
			float angle = Mathf.Deg2Rad * (60 * i) + rotationOffset; // ������ -30�� ȸ��
			vertices[i] = new Vector3(
				center.x + radius * Mathf.Cos(angle),
				center.y,
				center.z + radius * Mathf.Sin(angle)
			);
		}

		// ���� ���� ������ ���������� �׸�
		for (int i = 0; i < 6; i++)
		{
			Vector3 start = vertices[i];
			Vector3 end = vertices[(i + 1) % 6]; // ������ ������ ù ��° ������ ����
			Gizmos.DrawLine(start, end);
		}
	}
}

[System.Serializable]
public class TerrainType
{
	public LayerMask terrainLayerMask;
	public int terrainPenalty;
}
