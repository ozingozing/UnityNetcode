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
			// 자신을 제외한 객체만 처리
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

				// 전진 이동 계산
				Vector3 forwardMovement = transform.forward * speed * speedPercent * Time.deltaTime;
				
				// `transform.position`으로 이동
				transform.position += forwardMovement;

				LookAtTarget(path.lookPoints[pathIndex]);
			}
			yield return null;
		}
	}

	void LookAtTarget(Vector3 targetPos)
	{
		// 목표 회전 값 계산
		Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);

		// 목표 회전 값에서 Y축만 가져오기
		float targetYAngle = targetRotation.eulerAngles.y;
	
		// 현재 회전 값 가져오기
		Vector3 currentEulerAngles = transform.rotation.eulerAngles;

		// 현재 Y축 회전 값과 목표 Y축 회전 값 보간
		float newYAngle = Mathf.LerpAngle(currentEulerAngles.y, targetYAngle, Time.deltaTime * turnSpeed);

		// 새로운 회전 값 생성 (Y축만 수정)
		Quaternion newRotation = Quaternion.Euler(0, newYAngle, 0);

		// 오브젝트 회전 설정
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
