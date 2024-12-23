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

		if (startNode.walkable && targetNode.walkable)
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
					print($"PathFound Using HEAP: {sw.ElapsedMilliseconds} ms");
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in gridGizmo.GetNeighbours(currentNode))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

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
		int q1 = nodeA.gridX, r1 = nodeA.gridY;
		int q2 = nodeB.gridX, r2 = nodeB.gridY;

		return Mathf.Max(Mathf.Abs(q1 - q2), Mathf.Abs(r1 - r2), Mathf.Abs(-(q1 + r1) + (q2 + r2)));
	}
}
