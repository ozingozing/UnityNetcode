using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{
	public bool turnOnHeap = false;
	GridGizmo gridGizmo;

	private void Awake()
	{
		gridGizmo = GetComponent<GridGizmo>();
	}

	public void FindPathHeap(PathRequest request, Action<PathResult> callback)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Node startNode = gridGizmo.NodeFromWorldPoint(request.pathStart);
		Node targetNode = gridGizmo.NodeFromWorldPoint(request.pathEnd);
		//If targetNode and neighbourNodes are Unwalkable, target is changed one of neighbours;
		if(!targetNode.walkable)
		{
			int min = int.MaxValue;
			Node tempNode = targetNode;
			foreach (Node item in gridGizmo.GetNeighbours2(targetNode, 3))
			{
				if (item.walkable)
				{
					int d = GetHexDistance(targetNode, item);
					if (d < min)
					{
						min = d;
						tempNode = item;
						//Debug Check
						//Instantiate(gridGizmo.Check, item.worldPosition, Quaternion.identity);
					}
				}
			}
			targetNode = tempNode;
		}
		/*if (startNode.walkable &&
			targetNode.walkable)*/
		{
			Heap<Node> openSet = new Heap<Node>(gridGizmo.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();

			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode)
				{
					sw.Stop();
					//Debug
					//print($"PathFound Using HEAP: {sw.ElapsedMilliseconds} ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in gridGizmo.GetNeighbours(currentNode))
				{
					//타겟이 UnWalkable이면 탐색에서 제외 하지만
					//가끔 타겟이 블럭 근처에 있어서 타겟 노드가 UnWalkable노드에
					//걸쳐서 Walkable로 쭉 가다가 타겟 노드만 UnWalkable인 형태가 돼서
					//탐색이 안되는 현상이 있음
					/*if (!neighbour.walkable ||
						closedSet.Contains(neighbour)) continue;*/

					if ((neighbour != targetNode && !neighbour.walkable) ||
						closedSet.Contains(neighbour)) continue;

					int newMovementCostToNeighbour =
						currentNode.gCost +
						GetHexDistance(currentNode, neighbour) +
						neighbour.movementPenalty;

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetHexDistance(neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else
							openSet.UpdateItem(neighbour);
					}
				}
			}
		}

		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
			pathSuccess = waypoints.Length > 0;
		}
		callback(new PathResult(waypoints, pathSuccess, request.callback));
	}

	Vector3[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;
	}

	Vector3[] SimplifyPath(List<Node> path)
	{
		List<Vector3> waypoints = new List<Vector3>();
		Vector2 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++)
		{
			Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
			if (directionNew != directionOld)
			{
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	int GetHexDistance(Node nodeA, Node nodeB)
	{
		Vector3Int cubeA = OffsetToCube(nodeA.gridX, nodeA.gridY);
		Vector3Int cubeB = OffsetToCube(nodeB.gridX, nodeB.gridY);

		// Cube 좌표계에서 거리 계산
		return Mathf.Max(
			Mathf.Abs(cubeA.x - cubeB.x),
			Mathf.Abs(cubeA.y - cubeB.y),
			Mathf.Abs(cubeA.z - cubeB.z)
		);
	}

	Vector3Int OffsetToCube(int q, int r)
	{
		int x = q - (r % 2 == 0 ? r / 2 : (r - 1) / 2); // 홀수/짝수 행에 따라 x 계산
		int z = r;
		int y = -x - z;

		return new Vector3Int(x, y, z);
	}
}
