using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static DG.Tweening.DOTweenAnimation;

public class GetExploded : NetworkBehaviour
{
	public float explosionForce = 10;  // ���߷�
	public float explosionRadius = 5f;   // ���� �ݰ�
	public float disableDelay = 5f;      // ������ ���ư� �� ��Ȱ��ȭ �ð�

	MeshRenderer mesh; //RootModle�� ChildModel�� ������ ���� �� ����

	GameObject ModelRoot;
	public GameObject prefab;
	public Rigidbody[] childPieces;     // �ڽ� ������Ʈ ��� ����
	private Vector3[] initialPositions;  // �ʱ� ��ġ ����
	private Quaternion[] initialRotations; // �ʱ� ȸ���� ����

	// Start is called before the first frame update
	private void Awake()
	{
		ModelRoot = Instantiate(prefab, transform.position, Quaternion.identity, transform);
		if (ModelRoot.TryGetComponent(out MeshRenderer mesh))
			this.mesh = mesh;

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
			if (mesh != null)
				childPieces[i].gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		ModelRoot.gameObject.SetActive(true);
	}

	#region Call NetcodeObject
	public async void Explode(Action<NetworkObject> deleteRequestCallback, NetworkObject networkObject)
	{
		Vector3 explosionPosition = transform.position;

		foreach (Rigidbody child in childPieces)
		{
			if (mesh != null)
				child.gameObject.SetActive(true);
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
		ModelRoot.gameObject.SetActive(false);

		double targetTime = NetworkManager.Singleton.ServerTime.Time + disableDelay;

		while (NetworkManager.Singleton.ServerTime.Time < targetTime)
		{
			await Task.Yield();
		}

		if (!IsServer)
			deleteRequestCallback.Invoke(networkObject);
	}
	#endregion

	#region Call NetcodeObjectId
	public async void ReceiveExplode(Action<ulong> deleteRequestCallback, ulong objId)
	{
		Vector3 explosionPosition = transform.position;

		foreach (Rigidbody child in childPieces)
		{
			if (mesh != null)
				child.gameObject.SetActive(true);
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
		ModelRoot.gameObject.SetActive(false);

		double targetTime = NetworkManager.Singleton.ServerTime.Time + disableDelay;

		while (NetworkManager.Singleton.ServerTime.Time < targetTime)
		{
			await Task.Yield();
		}

		if(!IsServer)
			deleteRequestCallback.Invoke(objId);
	}
	#endregion

	public override void OnNetworkDespawn()
	{
		if (ModelRoot.transform.childCount == 0)
			ResetAgain();
		base.OnNetworkDespawn();
	}

	public void ResetAgain()
	{
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
				if (mesh != null)
					child.gameObject.SetActive(false);
			}
		}

		for (int i = 0; i < childPieces.Length; i++)
		{
			Transform child = childPieces[i].gameObject.transform;
			child.localPosition = initialPositions[i];
			child.localRotation = initialRotations[i];
		}
		ModelRoot.transform.localPosition = Vector3.zero;
		ModelRoot.transform.localRotation = Quaternion.identity;
	}
}
