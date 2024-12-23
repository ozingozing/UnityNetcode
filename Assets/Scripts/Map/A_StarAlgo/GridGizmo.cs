using System.Collections.Generic;
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

		CreateGrid();
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int r = 0; r < gridSizeY; r++)
		{
			for (int q = 0; q < gridSizeX; q++)
			{
				// Ȧ�� �࿡ xOffset �߰��Ͽ� �¹����� ��ġ
				float xOffset = (r % 2 == 0) ? 0 : hexWidth * 0.5f;

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
		BlurPenaltyMapHex(3);
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();
		int[] dq = { 1, -1, 0, 0, 1, -1 };
		int[] dr = { 0, 0, 1, -1, -1, 1 };

		for (int i = 0; i < 6; i++)
		{
			int checkQ = node.gridX + dq[i];
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

	private void BlurPenaltyMapHex(int blurSize)
	{
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
		int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

		// ���� �н�: q ����
		for (int r = 0; r < gridSizeY; r++)
		{
			for (int q = -kernelExtents; q <= kernelExtents; q++)
			{
				int sampleQ = Mathf.Clamp(q, 0, gridSizeX - 1);
				penaltiesHorizontalPass[0, r] += grid[sampleQ, r].movementPenalty;
			}

			for (int q = 1; q < gridSizeX; q++)
			{
				int removeIndex = Mathf.Clamp(q - kernelExtents - 1, 0, gridSizeX - 1);
				int addIndex = Mathf.Clamp(q + kernelExtents, 0, gridSizeX - 1);

				penaltiesHorizontalPass[q, r] = penaltiesHorizontalPass[q - 1, r]
											  - grid[removeIndex, r].movementPenalty
											  + grid[addIndex, r].movementPenalty;
			}
		}

		// ���� �н�: r ����
		for (int q = 0; q < gridSizeX; q++)
		{
			for (int r = -kernelExtents; r <= kernelExtents; r++)
			{
				int sampleR = Mathf.Clamp(r, 0, gridSizeY - 1);
				penaltiesVerticalPass[q, 0] += penaltiesHorizontalPass[q, sampleR];
			}

			int blurredPenalty = Mathf.RoundToInt(penaltiesVerticalPass[q, 0] / (kernelSize * kernelSize));
			grid[q, 0].movementPenalty = blurredPenalty;

			for (int r = 1; r < gridSizeY; r++)
			{
				int removeIndex = Mathf.Clamp(r - kernelExtents - 1, 0, gridSizeY - 1);
				int addIndex = Mathf.Clamp(r + kernelExtents, 0, gridSizeY - 1);

				penaltiesVerticalPass[q, r] = penaltiesVerticalPass[q, r - 1]
											- penaltiesHorizontalPass[q, removeIndex]
											+ penaltiesHorizontalPass[q, addIndex];

				blurredPenalty = Mathf.RoundToInt(penaltiesVerticalPass[q, r] / (kernelSize * kernelSize));
				grid[q, r].movementPenalty = blurredPenalty;

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
				Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
				// �������� ���� ���� (walkable ���ο� ����)
				Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;

				// �������� �׸���
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
