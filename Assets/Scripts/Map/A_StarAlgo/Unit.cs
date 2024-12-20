using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    float speed = 20;
    Vector3[] path;
    int targetIndex=0;

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

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if(pathSuccessful)
		{
			path = newPath;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
	{
		Vector3 curretnWaypoint = path[0];

		while (true)
		{
			if(transform.position == curretnWaypoint)
			{
				targetIndex++;
				if(targetIndex >= path.Length)
				{
					yield break;
				}
				curretnWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position, curretnWaypoint, speed * Time.deltaTime);
			yield return null;
		}
	}

	public void OnDrawGizmos()
	{
		if(path != null)
		{
			for(int i = targetIndex; i < path.Length; i++)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(path[i] + Vector3.up, Vector3.one / 2);

				Gizmos.color = Color.red;
				if (i == targetIndex)
				{
					Gizmos.DrawLine(transform.position + Vector3.up, path[i] + Vector3.up);
				}
				else
				{
					Gizmos.DrawLine(path[i - 1] + Vector3.up, path[i] + Vector3.up);
				}
			}
		}
	}
}
