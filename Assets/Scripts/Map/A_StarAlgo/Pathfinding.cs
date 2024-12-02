using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class Pathfinding : MonoBehaviour
{
	public Transform seeker, tartget;
    GridGizmo gridGizmo;
	private void Awake()
	{
		gridGizmo = GetComponent<GridGizmo>();
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.O))
		{
			if (tartget != null && seeker != null)
				FindPathList(seeker.position, tartget.position);
		}
		else if(Input.GetKeyDown(KeyCode.P))
		{
			if (tartget != null && seeker != null)
				FindPathHeap(seeker.position, tartget.position);
		}
	}

	void FindPathHeap(Vector3 startPos, Vector3 targetPos)
	{
		Stopwatch sw = new Stopwatch();
		sw.Start();

		Node startNode = gridGizmo.NodeFromWorldPoint(startPos);
		Node targetNode = gridGizmo.NodeFromWorldPoint(targetPos);

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
				RetracePath(startNode, targetNode);
				return;
			}

			foreach (Node neighbour in gridGizmo.GetNeighbours(currentNode))
			{
				if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
						openSet.Add(neighbour);
				}
			}
		}
	}

	void FindPathList(Vector3 startPos, Vector3 targetPos)
    {
		Stopwatch sw = new Stopwatch();
		sw.Start();

		Node startNode = gridGizmo.NodeFromWorldPoint(startPos);
		Node targetNode = gridGizmo.NodeFromWorldPoint(targetPos);

		List<Node> openSet = new List<Node>();
		HashSet<Node> closedSet = new HashSet<Node>();
		openSet.Add(startNode);

		while(openSet.Count > 0)
		{
			Node currentNode = openSet[0];
			//List Serch Time O(n) => we change heap
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
				RetracePath(startNode, targetNode);
				return;
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
	}

	void RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		while(currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		path.Reverse();

		gridGizmo.path = path;
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
