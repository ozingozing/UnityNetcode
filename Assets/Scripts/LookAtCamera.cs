using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	private Camera playerCamera;

	private void Start()
	{
		// �÷��̾��� ī�޶� ã�� ����
		playerCamera = Camera.main; // �⺻ ī�޶� ���
	}

	private void LateUpdate()
	{
		if (playerCamera != null)
		{
			// UI�� ī�޶� ���ϵ��� ȸ��
			transform.LookAt(transform.position + playerCamera.transform.rotation * Vector3.forward,
							 playerCamera.transform.rotation * Vector3.up);
		}
	}
}
