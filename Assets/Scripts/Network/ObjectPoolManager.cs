using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public GameObject Get()
    {
        if(pool.Count > 0)
        {
            GameObject instance = pool.Dequeue();
            instance.SetActive(false);
            return instance;
        }
        else
        {
            GameObject gameObject = Instantiate(prefab);
			NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
			if (networkObject != null)
			{
				networkObject.Spawn();
			}
			return gameObject;
        }
    }

    public void ReturnToPool(GameObject instance)
    {
        instance.SetActive(false);
        pool.Enqueue(instance);
    }
}
