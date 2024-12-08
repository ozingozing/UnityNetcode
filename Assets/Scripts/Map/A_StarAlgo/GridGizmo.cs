using System.Collections.Generic;
using UnityEngine;

public class GridGizmo : MonoBehaviour
{
	public bool onlyDisplayPathGizmo;
	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public LayerMask walkableMask;
	public Dictionary<int, int> walkableRegionDictionary = new Dictionary<int, int>();
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	public int MaxSize
	{
		get { return gridSizeX * gridSizeY; }
	}

	private void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		foreach(TerrainType region in walkableRegions)
		{
			walkableMask.value |= region.terrainLayerMask;
			walkableRegionDictionary.Add((int)Mathf.Log(region.terrainLayerMask.value, 2), region.terrainPenalty);
		}

		// Dictionary의 모든 key와 value를 출력
		foreach (KeyValuePair<int, int> entry in walkableRegionDictionary)
		{
			Debug.Log("Key: " + entry.Key + ", Value: " + entry.Value);
		}

		CreateGrid();
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridSizeY/2;
		for (int x = 0; x < gridSizeX; x++)
		{
			for(int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

				int n = 0;
				//raycast
				if(walkable)
				{
					Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
					RaycastHit hit;
					if (Physics.Raycast(ray, out hit, 100, walkableMask))
					{
						walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out int movementPenalty);
						n = movementPenalty;
					}
				}
				grid[x, y] = new Node(walkable, worldPoint, x, y, n);
			}
		}
	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for(int x = -1; x <= 1;  x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0) continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if((checkX >= 0 && checkX < gridSizeX)
				&& (checkY >= 0 && checkY < gridSizeY))
				{
					neighbours.Add(grid[checkX, checkY]);
				}
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

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (grid != null && onlyDisplayPathGizmo)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}
	}

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainLayerMask;
		public int terrainPenalty;
	}

}
