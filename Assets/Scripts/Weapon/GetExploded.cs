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
	public float explosionForce = 10;  // 폭발력
	public float explosionRadius = 5f;   // 폭발 반경
	public float disableDelay = 5f;      // 조각이 날아간 후 비활성화 시간

	MeshRenderer mesh; //RootModle과 ChildModel로 나눠져 있을 시 부착

	GameObject ModelRoot;
	public GameObject prefab;
	public Rigidbody[] childPieces;     // 자식 오브젝트 목록 저장
	private Vector3[] initialPositions;  // 초기 위치 저장
	private Quaternion[] initialRotations; // 초기 회전값 저장

	// Start is called before the first frame update
	private void Awake()
	{
		ModelRoot = Instantiate(prefab, transform.position, Quaternion.identity, transform);
		if (ModelRoot.TryGetComponent(out MeshRenderer mesh))
			this.mesh = mesh;

		// 초기 상태 저장 (한 번만 실행)
		int childCount = ModelRoot.transform.childCount;
		childPieces = new Rigidbody[childCount];
		initialPositions = new Vector3[childCount];
		initialRotations = new Quaternion[childCount];

		for (int i = 0; i < childCount; i++)
		{
			childPieces[i] = ModelRoot.transform.GetChild(i).GetComponent<Rigidbody>();
			initialPositions[i] = childPieces[i].gameObject.transform.localPosition;  // 초기 위치 저장
			initialRotations[i] = childPieces[i].gameObject.transform.localRotation;  // 초기 회전값 저장
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
			// 폭발력 적용
			child.isKinematic = false;

			// 랜덤 회전력 추가 (토크 적용)
			Vector3 randomTorque = new Vector3(
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f)
			) * explosionForce;  // 폭발력 크기에 비례하여 설정
			child.AddTorque(randomTorque, ForceMode.Impulse);
			Vector3 explosionDirection = ((child.position - explosionPosition) + (Vector3.up * .25f)).normalized;
			child.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
			// 부모에서 분리
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
			// 폭발력 적용
			child.isKinematic = false;

			// 랜덤 회전력 추가 (토크 적용)
			Vector3 randomTorque = new Vector3(
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f),
				UnityEngine.Random.Range(-1f, 1f)
			) * explosionForce;  // 폭발력 크기에 비례하여 설정
			child.AddTorque(randomTorque, ForceMode.Impulse);
			Vector3 explosionDirection = ((child.position - explosionPosition) + (Vector3.up * .25f)).normalized;
			child.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
			// 부모에서 분리
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
		// 조각들을 원래 위치로 되돌리기
		for (int i = 0; i < childPieces.Length; i++)
		{
			Rigidbody child = childPieces[i];

			if (child != null)
			{
				child.isKinematic = false;
				child.velocity = Vector3.zero; // 속도 초기화
				child.angularVelocity = Vector3.zero; // 회전 속도 초기화
				child.isKinematic = true; // 다시 물리 적용 해제

				// 위치와 회전 복구
				child.gameObject.transform.SetParent(ModelRoot.transform);  // 다시 부모에 부착
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
