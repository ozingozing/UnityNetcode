using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;

	Path path;

	private void Start()
	{
		StartCoroutine(WaitTarget());
	}
	
	IEnumerator WaitTarget()
	{
		while (target == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(20);
		PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
	}

	public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
	{
		if(pathSuccessful)
		{
			path = new Path(waypoints, transform.position, turnDst);
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
	{
		bool followingPath = true;
		int pathIndex = 0;
		transform.LookAt(path.lookPoints[0]);

		while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while(path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if (pathIndex == path.finishLineIndex)
				{
					followingPath = false;
					break;
				}
				else
					pathIndex++;
			}

			if(followingPath)
			{
				Quaternion targetRoation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRoation, Time.deltaTime * turnSpeed);
				transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
			}
			yield return null;
		}
	}

	public void OnDrawGizmos()
	{
		if(path != null)
		{
			path.DrawWithGizmos();
		}
	}
}
