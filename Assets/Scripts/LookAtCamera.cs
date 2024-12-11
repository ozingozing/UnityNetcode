using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private Camera playerCamera;

	private void Start()
	{
		// 플레이어의 카메라를 찾아 저장
		playerCamera = Camera.main; // 기본 카메라를 사용
	}

	private void LateUpdate()
	{
		if (playerCamera != null)
		{
			// UI가 카메라를 향하도록 회전
			transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
							 playerCamera.transform.rotation * Vector3.up);
		}
	}
}
