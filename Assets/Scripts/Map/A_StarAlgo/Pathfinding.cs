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
			//Heap(Prioriy Queue) spend time O(logN) when it Del or Add
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
						currentNode.gCost 
						+ GetDistance(currentNode, neighbour)
						+ neighbour.movementPenalty;

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
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
		}
		callback(new PathResult(waypoints, pathSuccess, request.callback));
	}

	void FindPathList(PathRequest request, Action<PathResult> callback)
    {
		Stopwatch sw = new Stopwatch();
		sw.Start();

		Node startNode = gridGizmo.NodeFromWorldPoint(request.pathStart);
		Node targetNode = gridGizmo.NodeFromWorldPoint(request.pathEnd);

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while(openSet.Count > 0)
		{
			Node currentNode = openSet[0];
			//List spend time O(n) when it Del or Add
			for (int i = 1; i < openSet.Count; i++)
			{
				if (openSet[i].fCost < currentNode.fCost
					|| (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
				{
					currentNode = openSet[i];
				}
			}
			openSet.Remove(currentNode);

			closedSet.Add(currentNode);

			if(currentNode == targetNode)
			{
				sw.Stop();
				print($"PathFound Using LIST: {sw.ElapsedMilliseconds} ms");
				pathSuccess = true;
				break;
			}

			foreach (Node neighbour in gridGizmo.GetNeighbours(currentNode))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if(!openSet.Contains(neighbour))
						openSet.Add(neighbour);
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
		while(currentNode != startNode)
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

		for(int i = 1; i < path.Count; i++)
		{
			Vector2 directionNew = new Vector2(path[i - 1].gridX, path[i - 1].gridY);
			if(directionNew != directionOld)
			{
				waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	int GetDistance(Node nodeA, Node nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		//14 * dstY + 10 * (dstX - dstY)
		//(14dstY + 10dstX - 10dstY)
		//(14 - 10)dstY + 10dstX => 4dstY + 10dstX
		if (dstX > dstY)
			return 4 * dstY + 10 * dstX;
		else
			return 4 * dstX + 10 * dstY; ;
	}
}
