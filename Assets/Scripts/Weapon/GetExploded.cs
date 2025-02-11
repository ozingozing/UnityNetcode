using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GetExploded : NetworkBehaviour
{
	public float explosionForce = 10;  // 폭발력
	public float explosionRadius = 5f;   // 폭발 반경
	public float disableDelay = 5f;      // 조각이 날아간 후 비활성화 시간

	Rigidbody rb;

	public GameObject ModelRoot;
	public GameObject prefab;
	public Rigidbody[] childPieces;     // 자식 오브젝트 목록 저장
	private Vector3[] initialPositions;  // 초기 위치 저장
	private Quaternion[] initialRotations; // 초기 회전값 저장

	// Start is called before the first frame update
	private void Awake()
	{
		ModelRoot = Instantiate(prefab, transform.position, Quaternion.identity, transform);
		rb = GetComponent<Rigidbody>();

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
		// 일정 시간 뒤 복구 코루틴 실행
		StartCoroutine(ResetAfterDelay(deleteRequestCallback, objId));
	}

	private IEnumerator ResetAfterDelay(Action<ulong> deleteRequestCallback, ulong objId)
	{
		rb.isKinematic = true;
		yield return new WaitForSeconds(disableDelay);

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

		// 전체 오브젝트 비활성화 (오브젝트 풀로 반환)
		if (!IsServer)
			deleteRequestCallback.Invoke(objId);
	}
}
