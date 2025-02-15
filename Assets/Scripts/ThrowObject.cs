using ChocoOzing.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ThrowObject : NetworkBehaviour
{
    public Vector3 setStartPoint;
    public Vector3 setEndPoint;
    public float duration = 2.0f;
    public float gravity = -9.8f;
    public Vector3 velocity;

	public Action<NetworkObjectReference> finishAction;

	/*public void ActionCall()
	{
		GetComponent<GetExploded>().Explode(DeleteRequestServerRpc, NetworkObjectId);
	}

	[ClientRpc]
	public void DoFinishingWorkClientRpc(ulong id)
	{
		if(!IsServer)
			DeleteRequestServerRpc(id);
	}

	int cntFinishAction = 0;
	[ServerRpc(RequireOwnership = false)]
	public void DeleteRequestServerRpc(ulong id)
	{
		if(++cntFinishAction == NetworkManager.ConnectedClientsList.Count - 1)
		{
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
			{
				if (obj.IsSpawned)
				{
					finishAction.Invoke(obj);
					cntFinishAction = 0;
				}
			}
		}
	}*/

	public async void TrowInit(Transform start, Vector3 end)
	{
		setStartPoint = start.position;
		setEndPoint = end;

		//������, ������ ���� �ʱ� �ӵ�
		Vector3 displacement = setEndPoint - setStartPoint;
        float time = duration;

        //x, y���� �ӵ�
        velocity.x = displacement.x / time;
        velocity.z = displacement.z / time;

        //y���� �ӵ�
        velocity.y = displacement.y / time - .5f * gravity * time;

		await Throw();
    }

	Node node;
	async Task Throw()
	{
		float elapsed = 0f;

		while (true)
		{
			elapsed += Time.deltaTime;

			// ������ ���Ŀ� ���� ��ġ ���
			Vector3 currentPosition = setStartPoint
				+ velocity * elapsed
				+ 0.5f * new Vector3(0, gravity, 0) * Mathf.Pow(elapsed, 2);

			//transform.rotation = Quaternion.LookRotation(velocity);
			transform.position = currentPosition;

			// ��ǥ ��ġ�� ���� ��ġ�� �Ÿ��� ����� ��������ٸ� ���� ����
			float threshold = Mathf.Max(0.1f, velocity.magnitude * Time.deltaTime);
			float distanceToTarget = Vector3.Distance(currentPosition, setEndPoint);
			if (distanceToTarget < threshold) // 0.1f�� ���� ���� (�ʿ� �� ����)
			{
				break;
			}

			await Task.Yield(); // ���� �����ӱ��� ���
		}

		// ��Ȯ�� �������� ����
		transform.position = setEndPoint;
		StartCoroutine(StartGridCheck());
	}

	private float fixedUpdateCount = 0;
	private const int CALL_INTERVAL = 1; // OneCall Per 60FPS  OneShot Per 1seconds
	IEnumerator StartGridCheck()
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			fixedUpdateCount += Time.fixedDeltaTime;
			if (fixedUpdateCount >= CALL_INTERVAL)
			{
				node = GridGizmo.instance.CheckAgain(transform.position);
				fixedUpdateCount = 0;
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		StopCoroutine(StartGridCheck());
		if (node != null)
		{
			node.ReturnToOriginValue();
		}
		base.OnNetworkDespawn();
	}
}
