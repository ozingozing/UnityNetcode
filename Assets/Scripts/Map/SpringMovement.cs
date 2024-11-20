using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SpringMovement : NetworkBehaviour
{
	public Vector3 target; // 목표 위치
	public float stiffness = 10f; // 스프링 강도
	public float damping = 5f; // 감쇠율
	public float mass = 1f; // 질량
	public float stopThreshold = 0.01f; // 멈추는 거리 임계값

	private Vector3 velocity = Vector3.zero; // 현재 속도

	public override void OnNetworkSpawn()
	{
		StartCoroutine(StartTrigger());
		base.OnNetworkSpawn();
	}

	IEnumerator StartTrigger()
	{
		while (true)
		{
			// 스프링 이동 계산
			Vector3 displacement = new Vector3(0, -transform.position.y, 0); // 목표와의 거리
			Vector3 springForce = stiffness * displacement; // 스프링 힘
			Vector3 dampingForce = -damping * velocity; // 감쇠 힘
			Vector3 acceleration = (springForce + dampingForce) / mass; // 가속도 = (스프링 + 감쇠) / 질량

			velocity += acceleration * Time.deltaTime; // 속도 업데이트
			transform.position += velocity * Time.deltaTime; // 위치 업데이트

			// 목표에 도착했다고 판단되면 Update 종료
			if (displacement.magnitude < stopThreshold && velocity.magnitude < stopThreshold)
			{
				yield break;
			}
			yield return null;
		}
	}
}
