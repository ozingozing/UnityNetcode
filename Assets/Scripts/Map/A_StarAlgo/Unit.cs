using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
	const float pathUpdateMoveThreshold = .5f;
	const float minPathUpdateTime = .5f;

	public LayerMask layerMask;
    public Transform target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float stoppingDst = 10;
	public float turnDst = 0;

	private Path path;
	private Rigidbody rb;
	private bool followingPath = false;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		turnDst = (GridGizmo.instance.hexRadius);
	}

	private void FixedUpdate()
	{
		if(!followingPath && target)
			LookAtTarget(target.position);
	}

	public void StartAction(GameObject go)
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 100f, layerMask);
		foreach (Collider hitCollider in hitColliders)
		{
			// �ڽ��� ������ ��ü�� ó��
			if (hitCollider.gameObject != go)
			{
				target = hitCollider.gameObject.transform;
				Debug.Log($"Detected: {target.name}");
				break;
			}
		}

		StartCoroutine(UpdatePath());
	}

	public void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
	{
		if(pathSuccessful)
		{
			path = new Path(waypoints, transform.position, turnDst, stoppingDst);
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator UpdatePath()
	{
		while (target == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(10);

		if (Time.timeSinceLevelLoad < .3f)
			yield return new WaitForSeconds(.3f);

		PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;
		while (true)
		{
			yield return new WaitForSeconds(minPathUpdateTime);
			if((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
			{
				PathRequestManager.RequestPath(
					new PathRequest(transform.position,
					target.position,
					OnPathFound));
				targetPosOld = target.position;
			}
		}
	}

	IEnumerator FollowPath()
	{
		followingPath = true;
		int pathIndex = 0;
		transform.LookAt(path.lookPoints[0]);

		float speedPercent = 1;

		while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if (pathIndex == path.finishLineIndex)
				{
					LookAtTarget(target.position);
					followingPath = false;
					break;
				}
				else
					pathIndex++;
			}

			if(followingPath)
			{
				if(pathIndex >= path.slowDownIndex && stoppingDst > 0)
				{
					//speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
					speedPercent = 1;
					if (speedPercent < 0.01f)
					{
						followingPath = false;
					}
				}

				// ���� �̵� ���
				Vector3 forwardMovement = transform.forward * speed * speedPercent * Time.deltaTime;
				
				// `transform.position`���� �̵�
				transform.position += forwardMovement;

				LookAtTarget(path.lookPoints[pathIndex]);
			}
			yield return null;
		}
	}

	void LookAtTarget(Vector3 targetPos)
	{
		// ��ǥ ȸ�� �� ���
		Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);

		// ��ǥ ȸ�� ������ Y�ุ ��������
		float targetYAngle = targetRotation.eulerAngles.y;
	
		// ���� ȸ�� �� ��������
		Vector3 currentEulerAngles = transform.rotation.eulerAngles;

		// ���� Y�� ȸ�� ���� ��ǥ Y�� ȸ�� �� ����
		float newYAngle = Mathf.LerpAngle(currentEulerAngles.y, targetYAngle, Time.deltaTime * turnSpeed);

		// ���ο� ȸ�� �� ���� (Y�ุ ����)
		Quaternion newRotation = Quaternion.Euler(0, newYAngle, 0);

		// ������Ʈ ȸ�� ����
		transform.rotation = newRotation;

		rb.angularVelocity = Vector3.zero;
	}

	public void OnDrawGizmos()
	{
		if(path != null)
		{
			path.DrawWithGizmos(target);
		}
	}
}
