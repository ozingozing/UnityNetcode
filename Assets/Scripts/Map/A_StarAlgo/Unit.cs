using Architecture.AbilitySystem.Model;
using ChocoOzing.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : NetworkBehaviour
{
	const float pathUpdateMoveThreshold = .5f;
	const float minPathUpdateTime = .5f;
	
	public GameObject Effect;
	public LayerMask layerMask;
    public Transform target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float stoppingDst = 10;
	public float turnDst = 0;

	public Path path;
	private Rigidbody rb;
	private bool followingPath = false;

	ObjectPool ParticlePool;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		turnDst = (GridGizmo.instance.hexRadius);
	}

	private void Start()
	{
		if (ParticlePool == null)
			ParticlePool = ObjectPool.CreateInstance(Effect.GetComponent<PoolableObject>(), 4);
	}

	private void OnEnable()
	{
		if(gameObject.activeSelf)
		{
			followingPath = false;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}

	private void FixedUpdate()
	{
		if(!followingPath && target)
			LookAtTarget(target.position);
	}

	float minDistance = float.MaxValue;
	/// <summary>
	/// Exclude OwnerPlayer And Start to Search
	/// </summary>
	/// <param name="OwnerPlayer">Exclude this one</param>
	/// <param name="findOnce">If you want to search Once?</param>
	public void StartAction(GameObject ownerPlayer)
	{
		minDistance = float.MaxValue;
		StartCoroutine(FindTarget(ownerPlayer));
		StartCoroutine(UpdatePath());
	}

	public void StartActionTest(GameObject go)
	{
		target = go.transform;
		StartCoroutine(UpdatePath());
	}

	IEnumerator FindTarget(GameObject ownerPlayer)
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 100f, layerMask);
			if (hitColliders.Length > 0)
			{
				foreach (Collider hitCollider in hitColliders)
				{
					// �ڽ��� ������ ��ü�� ó��
					if (hitCollider.gameObject != ownerPlayer)
					{
						float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
						if (distance < minDistance)
						{
							minDistance = distance;
							target = hitCollider.gameObject.transform;
						}
					}
				}
				yield break;
			}
		}
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
			if (target != null &&
				(target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
			{
				PathRequestManager.RequestPath(
					new PathRequest(transform.position,
					target.position,
					OnPathFound));
				targetPosOld = target.position;
			}
		}
	}

	public void OnDisable()
	{
		/*if (ParticlePool == null)
			ParticlePool = ObjectPool.CreateInstance(Effect.GetComponent<PoolableObject>(), 4);
		ParticlePool.GetObject(transform.position, Quaternion.identity);
		FinishAction = null;
		transform.GetChild(0).GetComponent<GetExploded>().Explode();*/
	}

	public void ActionCall(Action<ulong> deleteRequestCallback, ulong objId)
	{
		ParticlePool.GetObject(transform.position, Quaternion.identity);
		FinishAction = null;
		transform.GetComponent<GetExploded>().Explode(deleteRequestCallback, objId);
	}

	public Action<ulong> FinishAction;
	IEnumerator FollowPath()
	{
		if(!gameObject.activeSelf) yield break;
		
		followingPath = true;
		int pathIndex = 0;
		float speedPercent = 1;

		transform.LookAt(path.lookPoints[0]);

		while (followingPath)
		{
			Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
			while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
			{
				if(target != null &&
				pathIndex == path.finishLineIndex)
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
					//Don't use speedPercent
					speedPercent = 1;
					if (speedPercent < 0.01f)
					{
						followingPath = false;
					}
				}

				LookAtTarget(path.lookPoints[pathIndex]);

				// ���� �̵� ���
				Vector3 forwardMovement = transform.forward * speed * speedPercent * Time.deltaTime;

				// `transform.position`���� �̵�
				transform.position += forwardMovement;
			}
			yield return null;
		}

		if(FinishAction !=	null)
			FinishAction.Invoke(GetComponent<NetworkObject>().NetworkObjectId);
	}

	void LookAtTarget(Vector3 targetPos)
	{
		if (targetPos - transform.position == Vector3.zero) return;
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
	}

	public void OnDrawGizmos()
	{
		if(path != null)
		{
			path.DrawWithGizmos(target);
		}
	}
}
