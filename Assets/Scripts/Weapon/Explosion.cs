using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using ChocoOzing.Utilities;

public class Explosion : MonoBehaviour
{
	//Noted: All NetworkRootObjects without player must include the collider in childOb but rb is fine
	public float explosionForce = 10;  // ���߷�
	public float explosionRadius = 5;   // ���� �ݰ�
	public LayerMask layerMask;

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			if (other.TryGetComponenInParent(out ThrowObject item))
			{
				item.ActionCall();
			}
		}
		if (other.gameObject.layer == LayerMask.NameToLayer("Extra"))
		{
			if (other.TryGetComponent(out Rigidbody rb))
			{
				// ���� ȸ���� �߰� (��ũ ����)
				Vector3 randomTorque = new Vector3(
					UnityEngine.Random.Range(-1f, 1f),
					UnityEngine.Random.Range(-1f, 1f),
					UnityEngine.Random.Range(-1f, 1f)
				) * explosionForce;  // ���߷� ũ�⿡ ����Ͽ� ����
				rb.AddTorque(randomTorque, ForceMode.Impulse);
				Vector3 explosionDirection = ((other.transform.position - transform.position) + (Vector3.up * 0.25f)).normalized;
				rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
			}
		}
		if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			if (other.TryGetComponent(out Rigidbody rb))
			{
				
				// ���� �������� �÷��̾� ���� ���� ���ϱ�
				Vector3 explosionDirection = (other.transform.position - transform.position).normalized;

				// �밢�� �������� �� ���� (x, z�� ���ϰ� / y�� �����ϰ�)
				Vector3 forceDirection = new Vector3(explosionDirection.x * 1.5f, 1.2f, explosionDirection.z * 1.5f);

				// �� ����
				rb.AddForce(forceDirection * 30, ForceMode.Impulse);
			}
		}
	}
}
