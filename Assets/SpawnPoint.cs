using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public List<Transform> SpawnPoints;  // Populate with spawn positions in-editor

	private NetworkManager m_NetworkManager;

	System.Random random = new System.Random();

	private void Awake()
	{
		m_NetworkManager = GetComponent<NetworkManager>();

		/*for(int i = 0; i < 3 ; i++)
		{
			SpawnPoints.Add(new Vector3(random.Next(0, 10), random.Next(0, 10), random.Next(0, 10)));
		}*/
	}

	public Vector3 GetRandomSpawnPoint()
	{
		if (SpawnPoints != null && SpawnPoints.Count > 0)
		{
			return SpawnPoints[(int)random.Next(0, SpawnPoints.Count - 1)].position;
		}
		return Vector3.zero;
	}
}
