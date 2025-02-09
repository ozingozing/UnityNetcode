using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using static UnityEngine.UI.Image;

public class RaycastBatchProcessor
{
	[SerializeField] int maxRaycastsPerJob = 10000;
	public static RaycastBatchProcessor Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new RaycastBatchProcessor();
			}
			return _instance;
		}
	}
	private static RaycastBatchProcessor _instance;


	NativeArray<RaycastCommand> rayCommands;
	NativeArray<RaycastHit> hitResults;

	public void PerformRaycasts(
		Vector3[] origins,
		Vector3[] directions,
		int layerMask,
		bool hitBackfaces,
		bool hitTriggers,
		bool hitMultiFace,
		Action<RaycastHit[]> callback)
	{
		const float maxDistance = 500f;
		int rayCount = Mathf.Min(origins.Length, maxRaycastsPerJob);

		QueryTriggerInteraction queryTriggerInteraction = hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
		/*NativeArray<T>�� Unity�� C# Job System�� Burst Compiler���� ����ϴ� ���� �迭 ������. 
		 * NativeArray�� **�������� �ʴ� �޸�(Unmanaged Memory)**�� �Ҵ��ϱ� ������ **������ �÷���(GC)**�� �������� �ʾ�. 
		 * �׷��� IDisposable �������̽��� �����ϰ�, Dispose() �޼��带 ���� ��������� �޸𸮸� �����ؾ� ��.*/
		using (rayCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
		{
			QueryParameters parameters = new QueryParameters
			{
				layerMask = layerMask,
				hitBackfaces = hitBackfaces,
				hitTriggers = queryTriggerInteraction,
				hitMultipleFaces = hitMultiFace
			};

			for (int i = 0; i < rayCount; i++)
			{
				rayCommands[i] = new RaycastCommand(origins[i], directions[i], parameters, maxDistance);
			}

			ExecuteRaycasts(rayCommands, callback);
		}
	}

	public void PerformRaycasts(
		Vector3 origin,
		Vector3[] directions,
		int layerMask,
		bool hitBackfaces,
		bool hitTriggers,
		bool hitMultiFace,
		Action<RaycastHit[]> callback)
	{
		const float maxDistance = 500f;
		int rayCount = Mathf.Min(directions.Length, maxRaycastsPerJob);

		QueryTriggerInteraction queryTriggerInteraction = hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

		using (rayCommands = new NativeArray<RaycastCommand>(rayCount, Allocator.TempJob))
		{
			QueryParameters parameters = new QueryParameters
			{
				layerMask = layerMask,
				hitBackfaces = hitBackfaces,
				hitTriggers = queryTriggerInteraction,
				hitMultipleFaces = hitMultiFace
			};

			for (int i = 0; i < rayCount; i++)
			{
				rayCommands[i] = new RaycastCommand(origin, directions[i], parameters, maxDistance);
			}

			ExecuteRaycasts(rayCommands, callback);
		}
	}

	void ExecuteRaycasts(NativeArray<RaycastCommand> raycastCommands, Action<RaycastHit[]> callback)
	{
		int maxHitsPerRaycast = 1;
		int totalHitsNeeded = raycastCommands.Length * maxHitsPerRaycast;

		using (hitResults = new NativeArray<RaycastHit>(totalHitsNeeded, Allocator.TempJob))
		{
			//Debug
			/*foreach (RaycastCommand t in raycastCommands)
			{
				Debug.DrawLine(t.from, t.from + t.direction * 1f, Color.red, 0.5f);
			}*/

			JobHandle raycastJobHandle = RaycastCommand.ScheduleBatch(raycastCommands, hitResults, maxHitsPerRaycast);
			raycastJobHandle.Complete();

			if (hitResults.Length > 0)
			{
				RaycastHit[] results = hitResults.ToArray();

				//Debug Points
				 /*for (int i = 0; i < results.Length; i++) {
				     if (results[i].collider != null) {
				         Debug.Log($"Hit: {results[i].collider.name} at {results[i].point}");
				         Debug.DrawLine(raycastCommands[i].from, results[i].point, Color.green, 1.0f);
				     }
				 }*/

				callback?.Invoke(results);
			}
		}
	}
}