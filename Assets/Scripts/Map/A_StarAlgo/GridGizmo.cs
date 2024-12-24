using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridGizmo : MonoBehaviour
{
	public bool displayGridGizmos;
	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float hexRadius; // �������� Ÿ�� ������
	public TerrainType[] walkableRegions;
	public int obstacleProximityPenalty = 10;
	public LayerMask walkableMask;
	public Dictionary<int, int> walkableRegionDictionary = new Dictionary<int, int>();
	Node[,] grid;

	float hexWidth, hexHeight, hexHorizontalSpacing, hexVerticalSpacing;
	int gridSizeX, gridSizeY;
	int penaltyMin = int.MaxValue;
	int penaltyMax = int.MinValue;

	public int MaxSize
	{
		get { return gridSizeX * gridSizeY; }
	}

	void Awake()
	{
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

		CreateGrid();
	}

	Vector3 worldBottomLeft;
	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int r = 0; r < gridSizeY; r++)
		{
			for (int q = 0; q < gridSizeX; q++)
			{
				// Ȧ�� �࿡ xOffset �߰��Ͽ� �¹����� ��ġ
				float xOffset = (r % 2 == 1) ? hexWidth * 0.5f : 0;

				// �������� �߽� ��ǥ ���
				Vector3 worldPoint = worldBottomLeft +
									 new Vector3(q * hexHorizontalSpacing + xOffset, 0, r * hexVerticalSpacing);

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
		BlurPenaltyMap(3);
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();
		//int[] dq = { 1,-1,  0,0,  1,-1 };
		//int[] dr = { 0,0,  1,-1,  -1,1 };

		int[] dq_even = { 1,-1,  1, 0,  1, 0 };
		int[] dq_odd =  { 1,-1,  0,-1,  0,-1 };
		int[] dr =	    { 0,0,   1,-1,  -1,1 };

		bool isOddRow = node.gridY % 2 == 0;

		for (int i = 0; i < 6; i++)
		{
			int checkQ = node.gridX + (isOddRow ? dq_odd[i] : dq_even[i]);
			int checkR = node.gridY + dr[i];

			if (checkQ >= 0 && checkQ < gridSizeX && checkR >= 0 && checkR < gridSizeY)
			{
				neighbours.Add(grid[checkQ, checkR]);
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int q = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int r = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[q, r];
	}

	private void BlurPenaltyMap(int blurSize)
	{
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
		int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

		for (int y = 0; y < gridSizeY; y++)
		{
			for (int x = -kernelExtents; x < kernelExtents; x++)
			{
				int sampleX = Mathf.Clamp(x, 0, kernelExtents);
				penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
			}

			for (int x = 1; x < gridSizeX; x++)
			{
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

				penaltiesHorizontalPass[x, y]
					= penaltiesHorizontalPass[x - 1, y]
					- grid[removeIndex, y].movementPenalty
					+ grid[addIndex, y].movementPenalty;
			}
		}

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = -kernelExtents; y < kernelExtents; y++)
			{
				int sampleY = Mathf.Clamp(y, 0, kernelExtents);
				penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
			}

			int blurredPenalty = Mathf.RoundToInt(penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
			grid[x, 0].movementPenalty = blurredPenalty;

			for (int y = 1; y < gridSizeY; y++)
			{
				int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
				int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

				penaltiesVerticalPass[x, y]
					= penaltiesVerticalPass[x, y - 1]
					- penaltiesHorizontalPass[x, removeIndex]
					+ penaltiesHorizontalPass[x, addIndex];
				blurredPenalty = Mathf.RoundToInt(penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
				grid[x, y].movementPenalty = blurredPenalty;

				if (blurredPenalty > penaltyMax)
				{
					penaltyMax = blurredPenalty;
				}
				if (blurredPenalty < penaltyMin)
				{
					penaltyMin = blurredPenalty;
				}
			}
		}
	}


	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null && displayGridGizmos)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = Color.Lerp(Color.green, Color.blue, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
				// �������� ���� ���� (walkable ���ο� ����)
				Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;

				DrawHexagon(n.worldPosition, hexRadius);
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

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainLayerMask;
		public int terrainPenalty;
	}
}
