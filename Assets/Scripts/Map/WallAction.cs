using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WallAction : NetworkBehaviour
{
	public Vector3 TargetScale = Vector3.one;
	public float stiffness = 10f;
	public float damping = 5f;
	public float mass = 1f;
	public float stopThreshold = 0.01f;

	private Vector3 scaleVelocity = Vector3.zero;
	private Vector3 velocity = Vector3.zero; // ���� �ӵ�

	private void Start()
	{
		TargetScale = transform.localScale + new Vector3(0, 30, 0);
	}

	public override void OnNetworkSpawn()
	{
		StartCoroutine(StartTrigger());
		base.OnNetworkSpawn();
	}

	public IEnumerator ScaleToTarger()
	{
		while (true)
		{
			Vector3 displacement = TargetScale - transform.localScale;
			Vector3 springForce = stiffness * displacement;
			Vector3 dampingForce = -damping * scaleVelocity;
			Vector3 acceleration = (springForce + dampingForce) / mass;

			scaleVelocity += acceleration * Time.deltaTime;
			transform.localScale += scaleVelocity * Time.deltaTime;

			if(displacement.magnitude > stopThreshold && scaleVelocity.magnitude < stopThreshold)
			{
				transform.localScale = TargetScale;
				yield break;
			}

			yield return null;
		}
	}

	IEnumerator StartTrigger()
	{
		while (true)
		{
			// ������ �̵� ���
			Vector3 displacement = new Vector3(0, -transform.position.y, 0); // ��ǥ���� �Ÿ�
			Vector3 springForce = stiffness * displacement; // ������ ��
			Vector3 dampingForce = -damping * velocity; // ���� ��
			Vector3 acceleration = (springForce + dampingForce) / mass; // ���ӵ� = (������ + ����) / ����

			velocity += acceleration * Time.deltaTime; // �ӵ� ������Ʈ
			transform.position += velocity * Time.deltaTime; // ��ġ ������Ʈ

			// ��ǥ�� �����ߴٰ� �ǴܵǸ� Update ����
			if (displacement.magnitude < stopThreshold && velocity.magnitude < stopThreshold)
			{
				StartCoroutine(ScaleToTarger());
				yield break;
			}
			yield return null;
		}
	}
}
