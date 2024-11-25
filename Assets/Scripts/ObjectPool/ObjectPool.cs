using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectPool
{
	private GameObject Parent;
	private PoolableObject Prefab;
	private int Size;
	private Queue<PoolableObject> AvailableObjectsPool = new Queue<PoolableObject>();
	private static Dictionary<PoolableObject, ObjectPool> ObjectPools = new Dictionary<PoolableObject, ObjectPool>();

	private ObjectPool(PoolableObject Prefab, int Size)
	{
		this.Prefab = Prefab;
		this.Size = Size;
	}

	public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
	{
		ObjectPool pool = null;

		if (ObjectPools.ContainsKey(Prefab))
		{
			pool = ObjectPools[Prefab];
		}
		else
		{
			pool = new ObjectPool(Prefab, Size);

			pool.Parent = new GameObject(Prefab + " Pool");
			pool.CreateObjects();

			ObjectPools.Add(Prefab, pool);
		}


		return pool;
	}

	private void CreateObjects()
	{
		for (int i = 0; i < Size; i++)
		{
			CreateObject();
		}
	}

	private void CreateObject()
	{
		PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, Parent.transform);
		poolableObject.Parent = this;
		poolableObject.gameObject.SetActive(false); // PoolableObject handles re-adding the object to the AvailableObjects
	}

	public PoolableObject GetObject(Vector3 Position, Quaternion Rotation)
	{
		if (AvailableObjectsPool.Count == 0) // auto expand pool size if out of objects
		{
			CreateObject();
		}

		if(AvailableObjectsPool.TryDequeue(out PoolableObject instance) && instance)
		{
			instance.transform.position = Position;
			instance.transform.rotation = Rotation;
			instance.gameObject.SetActive(true);
			return instance;
		}
		return null;
	}

	public PoolableObject GetObject()
	{
		return GetObject(Vector3.zero, Quaternion.identity);
	}

	public void ReturnObjectToPool(PoolableObject Object)
	{
		AvailableObjectsPool.Enqueue(Object);
	}
}