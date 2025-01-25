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

    bool lastWalkable;
    int lastMovementPenalty;

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

        //save origin setting
        lastWalkable = _walkable;
        lastMovementPenalty = _movementPanalty;
    }

	#region DynamicValueSet
	public void ReSetMovementPenalty(int _penalty, bool IsInit = true)
    {
        if (IsInit)
        {
            movementPenalty = _penalty;
            lastMovementPenalty = _penalty;
        }
        else
            movementPenalty = _penalty;
	}

    public void ReSetWalkable(bool _walkable, bool IsInit = true)
    {
        if(IsInit)
        {
            walkable = _walkable;
            lastWalkable = _walkable;
		}
        else
            walkable = _walkable;
    }

	List<Node> penaltiesTemp;
    public void SetpenaltiesTemp(Node _penaltiesTemp)
    {
        if(penaltiesTemp == null)
        {
			penaltiesTemp = new List<Node>();
		}

		penaltiesTemp.Add(_penaltiesTemp);
	}

	public void ReturnToOriginValue()
    {
        foreach (Node item in penaltiesTemp)
            item.SetOriginValue();

        penaltiesTemp.Clear();
    }

    void SetOriginValue()
    {
        walkable = lastWalkable;
        movementPenalty = lastMovementPenalty;
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
