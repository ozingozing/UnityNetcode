using System.Collections;
using System.Collections.Generic;
using ChocoOzing.Utilities;
using UnityEngine;

public class Path
{
    public readonly Vector3[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDst)
	{
		this.lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = Vector3Extensions.Vector3ToVector2(startPos);
	    for(int i = 0; i< lookPoints.Length; i++)
        {
            Vector2 currentPoint = Vector3Extensions.Vector3ToVector2(lookPoints[i]);
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoint = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
            turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint);
            previousPoint = turnBoundaryPoint;      
        }
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }

        Gizmos.color = Color.white;
        foreach (Line l in turnBoundaries)
        {
            l.DrawWithGizmos(10);
        }
    }
}
