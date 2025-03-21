using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class SurfaceManager : NetworkBehaviour
{
    private static SurfaceManager _instance;
    public static SurfaceManager Instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

	private void Awake()
	{
		if(Instance != null)
        {
            Debug.LogError("More than one SurfaceManager active in th scen! Destroying latest one: " + name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
	}

    [SerializeField]
    private List<SurfaceType> Surfaces = new List<SurfaceType>();
    [SerializeField]
    private int DefaultPoolSizes = 10;
    [SerializeField]
    private Surface DefaultSurface;

	public void HandleImpact(GameObject HitObject, Vector3 HitPoint, Vector3 HitNormal, ImpactType Impact, int TriangleIndex)
	{
		if (HitObject.TryGetComponent<Terrain>(out Terrain terrain))
		{
			List<TextureAlpha> activeTextures = GetActiveTexturesFromTerrain(terrain, HitPoint);
			foreach (TextureAlpha activeTexture in activeTextures)
			{
				SurfaceType surfaceType = Surfaces.Find(surface => surface.Albedo == activeTexture.Texture);
				if (surfaceType != null)
				{
					foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
					{
						if (typeEffect.ImpactType == Impact)
						{
							PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, activeTexture.Alpha);
						}
					}
				}
				else
				{
					foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
					{
						if (typeEffect.ImpactType == Impact)
						{
							PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
						}
					}
				}
			}
		}
		else if (HitObject.TryGetComponent<Renderer>(out Renderer renderer))
		{
			Texture activeTexture = GetActiveTextureFromRenderer(renderer, TriangleIndex);

			SurfaceType surfaceType = Surfaces.Find(surface => surface.Albedo == activeTexture);
			if (surfaceType != null)
			{
				foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.Surface.ImpactTypeEffects)
				{
					if (typeEffect.ImpactType == Impact)
					{
						PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
					}
				}
			}
			else
			{
				foreach (Surface.SurfaceImpactTypeEffect typeEffect in DefaultSurface.ImpactTypeEffects)
				{
					if (typeEffect.ImpactType == Impact)
					{
						PlayEffects(HitPoint, HitNormal, typeEffect.SurfaceEffect, 1);
					}
				}
			}
		}
	}

	private List<TextureAlpha> GetActiveTexturesFromTerrain(Terrain Terrain, Vector3 HitPoint)
	{
		Vector3 terrainPosition = HitPoint - Terrain.transform.position;
		Vector3 splatMapPosition = new Vector3(
			terrainPosition.x / Terrain.terrainData.size.x,
			0,
			terrainPosition.z / Terrain.terrainData.size.z
		);

		int x = Mathf.FloorToInt(splatMapPosition.x * Terrain.terrainData.alphamapWidth);
		int z = Mathf.FloorToInt(splatMapPosition.z * Terrain.terrainData.alphamapHeight);

		float[,,] alphaMap = Terrain.terrainData.GetAlphamaps(x, z, 1, 1);

		List<TextureAlpha> activeTextures = new List<TextureAlpha>();
		for (int i = 0; i < alphaMap.Length; i++)
		{
			if (alphaMap[0, 0, i] > 0)
			{
				activeTextures.Add(new TextureAlpha()
				{
					Texture = Terrain.terrainData.terrainLayers[i].diffuseTexture,
					Alpha = alphaMap[0, 0, i]
				});
			}
		}

		return activeTextures;
	}
	private Dictionary<Renderer, Texture> subMeshTextureCacheList = new Dictionary<Renderer, Texture>();
	private const int MaxCacheSize = 100; // 최대 캐시 크기
	private LinkedList<Renderer> cacheOrder = new LinkedList<Renderer>();

	private Texture GetActiveTextureFromRenderer(Renderer Renderer, int TriangleIndex)
	{
		if (Renderer.TryGetComponent<MeshFilter>(out MeshFilter meshFilter))
		{
			if (subMeshTextureCacheList.TryGetValue(Renderer, out Texture cachedTexture))
			{
				Debug.Log("Cache hit!");
				cacheOrder.Remove(Renderer);
				cacheOrder.AddFirst(Renderer);
				return cachedTexture;
			}

			Mesh mesh = meshFilter.mesh;

			if (mesh.subMeshCount > 1)
			{
				int[] hitTriangleIndices = new int[]
				{
					mesh.triangles[TriangleIndex * 3],
					mesh.triangles[TriangleIndex * 3 + 1],
					mesh.triangles[TriangleIndex * 3 + 2]
				};

				for (int i = 0; i < mesh.subMeshCount; i++)
				{
					int[] submeshTriangles = mesh.GetTriangles(i);
					for (int j = 0; j < submeshTriangles.Length; j += 3)
					{
						if (submeshTriangles[j] == hitTriangleIndices[0]
							&& submeshTriangles[j + 1] == hitTriangleIndices[1]
							&& submeshTriangles[j + 2] == hitTriangleIndices[2])
						{
							AddToCache(Renderer, Renderer.sharedMaterials[i].mainTexture);
							Debug.Log("SubMesh texture cached!");
							return Renderer.sharedMaterials[i].mainTexture;
						}
					}
				}
			}
			else
			{
				AddToCache(Renderer, Renderer.sharedMaterial.mainTexture);
				Debug.Log("Single Mesh texture cached!");
				return Renderer.sharedMaterial.mainTexture;
			}
		}

		Debug.LogError($"{Renderer.name} has no MeshFilter! Using default impact effect instead of texture-specific one because we'll be unable to find the correct texture!");
		return null;
	}

	private void AddToCache(Renderer render, Texture texture)
	{
        if (subMeshTextureCacheList.ContainsKey(render))
        {
			cacheOrder.Remove(render);
			cacheOrder.AddFirst(render);
			return;
        }

		subMeshTextureCacheList[render] = texture;
		cacheOrder.AddFirst(render);

		if(cacheOrder.Count > MaxCacheSize)
		{
			Renderer leastUsedRenderer = cacheOrder.Last.Value;
			cacheOrder.RemoveLast();
			subMeshTextureCacheList.Remove(leastUsedRenderer);
			Debug.Log($"Cache evicted for render: {leastUsedRenderer}");
		}
    }

	private void PlayEffects(Vector3 HitPoint, Vector3 HitNormal, SurfaceEffect SurfaceEffect, float SoundOffset)
	{
		foreach (SpawnObjectEffect spawnObjectEffect in SurfaceEffect.SpawnObjectEffects)
		{
			if (spawnObjectEffect.Probability > Random.value)
			{
				ObjectPool pool = ObjectPool.CreateInstance(spawnObjectEffect.Prefab.GetComponent<PoolableObject>(), DefaultPoolSizes);

				PoolableObject instance = pool.GetObject(HitPoint + HitNormal * 0.001f, Quaternion.LookRotation(HitNormal));
				
				instance.transform.forward = HitNormal;
				if (spawnObjectEffect.RandomizeRotation)
				{
					Vector3 offset = new Vector3(
						Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.x),
						Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.y),
						Random.Range(0, 180 * spawnObjectEffect.RandomizedRotationMultiplier.z)
					);

					instance.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
				}
			}
		}

		foreach (PlayAudioEffect playAudioEffect in SurfaceEffect.PlayAudioEffects)
		{
			AudioClip clip = playAudioEffect.AudioClips[Random.Range(0, playAudioEffect.AudioClips.Count)];
			ObjectPool pool = ObjectPool.CreateInstance(playAudioEffect.AudioSourcePrefab.GetComponent<PoolableObject>(), DefaultPoolSizes);
			AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();

			audioSource.transform.position = HitPoint;
			audioSource.PlayOneShot(clip, SoundOffset * Random.Range(playAudioEffect.VolumeRange.x, playAudioEffect.VolumeRange.y));
			StartCoroutine(DisableAudioSource(audioSource, clip.length + 2f));
		}
	}

	private IEnumerator DisableAudioSource(AudioSource AudioSource, float Time)
	{
		yield return new WaitForSeconds(Time);

		AudioSource.gameObject.SetActive(false);
	}

	private class TextureAlpha
	{
		public float Alpha;
		public Texture Texture;
	}
}
