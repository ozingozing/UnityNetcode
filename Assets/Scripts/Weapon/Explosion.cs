using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public float explosionForce = 10;  // 폭발력
	public float explosionRadius = 5;   // 폭발 반경

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			if(other.TryGetComponent(out ThrowObject item))
				item.RequestDelete();
		}
		if (other.gameObject.layer == LayerMask.NameToLayer("Extra"))
		{
			if (other.TryGetComponent(out Rigidbody rb))
			{
				// 랜덤 회전력 추가 (토크 적용)
				Vector3 randomTorque = new Vector3(
					UnityEngine.Random.Range(-1f, 1f),
					UnityEngine.Random.Range(-1f, 1f),
					UnityEngine.Random.Range(-1f, 1f)
				) * explosionForce;  // 폭발력 크기에 비례하여 설정
				rb.AddTorque(randomTorque, ForceMode.Impulse);
				Vector3 explosionDirection = ((other.transform.position - transform.position) + (Vector3.up * 0.25f)).normalized;
				rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
			}
		}
		/*if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			if (other.TryGetComponent(out Rigidbody rb))
			{
				// 폭발 지점에서 플레이어 방향 벡터 구하기
				Vector3 explosionDirection = (other.transform.position - transform.position).normalized;

				// 대각선 방향으로 힘 조정 (x, z를 강하게 / y는 적절하게)
				Vector3 forceDirection = new Vector3(explosionDirection.x * 1.5f, 1.2f, explosionDirection.z * 1.5f);

				// 힘 적용
				rb.AddForce(forceDirection * 30, ForceMode.Impulse);
			}
		}*/
	}
}
