using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool walkable { get; private set; }
	public int movementPenalty { get; private set; }

	public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    bool originWalkable;
    int originMovementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public bool tempCheck = false;

	public Node(bool _walkable,  Vector3 _worldPosition, int _gridX, int _gridY, int _movementPanalty)
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPanalty;

        //save origin setting
        originWalkable = _walkable;
        originMovementPenalty = _movementPanalty;
    }

    #region DynamicValueSet
    public void ReSetMovementPenalty(int _penalty, bool IsInit = false)
    {
        if (IsInit)
        {
            movementPenalty = _penalty;
            originMovementPenalty = _penalty;
        }
        else
            movementPenalty = _penalty;
	}

    public void ReSetWalkable(bool _walkable, bool IsInit = false)
    {
        if(IsInit)
        {
            walkable = _walkable;
            originWalkable = _walkable;
		}
        else
            walkable = _walkable;
    }

    public Node Owner;
	List<Node> penaltiesTemp = new List<Node>();
	public void SetpenaltiesTemp(Node _penaltiesTemp)
    {
		penaltiesTemp.Add(_penaltiesTemp);
	}

	public void ReturnToOriginValue()
    {
        foreach (Node item in penaltiesTemp)
        {
            if(item.Owner == this)
			    item.SetOriginValue();
		}

		penaltiesTemp.Clear();
    }

    void SetOriginValue()
    {
		walkable = originWalkable;
		movementPenalty = originMovementPenalty;
		tempCheck = false;
	}
    #endregion

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
