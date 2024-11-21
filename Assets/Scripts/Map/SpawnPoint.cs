using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public List<Vector3> SpawnPoints = new List<Vector3>();  // Populate with spawn positions in-editor

	int currentNum = -1;
	int beforeNum = -1;
	public Vector3 GetRandomSpawnPoint()
	{
		if (SpawnPoints != null && SpawnPoints.Count > 0)
		{
			currentNum = UnityEngine.Random.Range(0, SpawnPoints.Count);
			if(currentNum == beforeNum)
			{
				for (int i = 0; i < SpawnPoints.Count; i++)
				{
					currentNum = UnityEngine.Random.Range(0, SpawnPoints.Count);
					if (currentNum == beforeNum) i -= 1;
					else break;
				}
			}

			beforeNum = currentNum;
			return SpawnPoints[currentNum];
		}
		return Vector3.zero;
	}
}
