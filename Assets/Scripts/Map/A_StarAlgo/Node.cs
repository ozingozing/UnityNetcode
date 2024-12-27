using System.Collections;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

	public Node(bool _walkable,  Vector3 _worldPosition, int _gridX, int _gridY, int _movementPanalty)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPanalty;
    }

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public int HeapIndex 
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    /// <summary>
    /// If TargetValue > myValue OR TargetValue is null +
    /// If TargetValue == myValue 0
    /// If TargetValue < myValue -
    /// </summary>
    /// <param name="nodeToCompare"></param>
    /// <returns></returns>
	public int CompareTo(Node nodeToCompare)
	{
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
        return -compare;
    }
}
