using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GetExploded : NetworkBehaviour
{
	public float explosionForce = 10;  // ���߷�
	public float explosionRadius = 5f;   // ���� �ݰ�
	public float disableDelay = 5f;      // ������ ���ư� �� ��Ȱ��ȭ �ð�

	Rigidbody rb;

	public GameObject ModelRoot;
	public GameObject prefab;
	public Rigidbody[] childPieces;     // �ڽ� ������Ʈ ��� ����
	private Vector3[] initialPositions;  // �ʱ� ��ġ ����
	private Quaternion[] initialRotations; // �ʱ� ȸ���� ����

	// Start is called before the first frame update
	private void Awake()
	{
		ModelRoot = Instantiate(prefab, transform.position, Quaternion.identity, transform);
		rb = GetComponent<Rigidbody>();

		// �ʱ� ���� ���� (�� ���� ����)
		int childCount = ModelRoot.transform.childCount;
		childPieces = new Rigidbody[childCount];
		initialPositions = new Vector3[childCount];
		initialRotations = new Quaternion[childCount];

		for (int i = 0; i < childCount; i++)
		{
			childPieces[i] = ModelRoot.transform.GetChild(i).GetComponent<Rigidbody>();
			initialPositions[i] = childPieces[i].gameObject.transform.localPosition;  // �ʱ� ��ġ ����
			initialRotations[i] = childPieces[i].gameObject.transform.localRotation;  // �ʱ� ȸ���� ����
		}
	}

	private void OnEnable()
	{
		rb.isKinematic = false;
		ModelRoot.gameObject.SetActive(true);
	}

	public void Explode(Action<ulong> deleteRequestCallback, ulong objId)
	{
		Vector3 explosionPosition = transform.position;

		foreach (Rigidbody child in childPieces)
		{
			// ���߷� ����
			child.isKinematic = false;
			
			// ���� ȸ���� �߰� (��ũ ����)
			Vector3 randomTorque = new Vector3(
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f)
			) * explosionForce;  // ���߷� ũ�⿡ ����Ͽ� ����
			child.AddTorque(randomTorque, ForceMode.Impulse);
			Vector3 explosionDirection = ((child.position - explosionPosition) + (Vector3.up * .25f)).normalized;
			child.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
			// �θ𿡼� �и�
			child.gameObject.transform.SetParent(null);
		}
		// ���� �ð� �� ���� �ڷ�ƾ ����
		StartCoroutine(ResetAfterDelay(deleteRequestCallback, objId));
	}

	private IEnumerator ResetAfterDelay(Action<ulong> deleteRequestCallback, ulong objId)
	{
		rb.isKinematic = true;
		yield return new WaitForSeconds(disableDelay);

		// �������� ���� ��ġ�� �ǵ�����
		for (int i = 0; i < childPieces.Length; i++)
		{
			Rigidbody child = childPieces[i];

			if (child != null)
			{
				child.isKinematic = false;
				child.velocity = Vector3.zero; // �ӵ� �ʱ�ȭ
				child.angularVelocity = Vector3.zero; // ȸ�� �ӵ� �ʱ�ȭ
				child.isKinematic = true; // �ٽ� ���� ���� ����

				// ��ġ�� ȸ�� ����
				child.gameObject.transform.SetParent(ModelRoot.transform);  // �ٽ� �θ� ����
			}
		}

		ModelRoot.gameObject.SetActive(false);
		
		for(int i = 0; i< childPieces.Length; i++)
		{
			Transform child = childPieces[i].gameObject.transform;
			child.localPosition = initialPositions[i];
			child.localRotation = initialRotations[i];
		}
		ModelRoot.transform.localPosition = Vector3.zero;
		ModelRoot.transform.localRotation = Quaternion.identity;

		// ��ü ������Ʈ ��Ȱ��ȭ (������Ʈ Ǯ�� ��ȯ)
		if (!IsServer)
			deleteRequestCallback.Invoke(objId);
	}
}
