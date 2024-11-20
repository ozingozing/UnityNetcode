using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpringMovement : NetworkBehaviour
{
	public Vector3 target; // ��ǥ ��ġ
	public float stiffness = 10f; // ������ ����
	public float damping = 5f; // ������
	public float mass = 1f; // ����
	public float stopThreshold = 0.01f; // ���ߴ� �Ÿ� �Ӱ谪

	private Vector3 velocity = Vector3.zero; // ���� �ӵ�

	public override void OnNetworkSpawn()
	{
		StartCoroutine(StartTrigger());
		base.OnNetworkSpawn();
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
				yield break;
			}
			yield return null;
		}
	}
}
