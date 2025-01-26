using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Network;
using ChocoOzing.Utilities;
using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class W : CoreComponent, ISkillAction
{
	public AbilityData abilityData {
		get => AbilityData;
		set {
			SetAbilityData(value);
		}
	}

	public bool isHoldAction {
		get => IsHoldAction;
		set => IsHoldAction = value;
	}

	[SerializeField] private bool IsHoldAction = true;
	[SerializeField] private AbilityData AbilityData;
	MyPlayer player;
	PlayerInit playerInit;
	ChocoOzing.Utilities.CountdownTimer coolTimer;

	public GameObject wallPrefab;
	public GameObject wallPreviewPrefab;
	public LayerMask buildableLayer;
	public LayerMask disbuildableLayer;
	public int maxWalls = 3;

	private GameObject currentPreview;
	private bool canBuild;
	private Vector3 lastValidPosition;
	private Queue<NetworkObject> builtWalls = new Queue<NetworkObject>();

	public void SetAbilityData(AbilityData abilityData)
	{
		AbilityData = abilityData;
		player = Core.Root.GetComponent<MyPlayer>();
		playerInit = Core.Root.GetComponent<PlayerInit>();
		coolTimer = Core.GetCoreComponent<AbilitySystem>().controller.cooltimer;
	}

	public void Action(PlayerAnimationEvent @evnet)
	{
		if(IsLocalPlayer)
		{
			isHoldAction = abilityData.isHoldAction ? true : false;
			playerInit.TurnOffCurrentWeaponServerRpc();
			StartCoroutine(MyAction());
		}
	}

	IEnumerator MyAction()
	{
		while(true)
		{
			if (coolTimer.IsRunning)
				coolTimer.Pause();
			yield return null;
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				isHoldAction = false;
				RequestSapwnServerRpc(currentPreview.transform.position);
				player.Anim.CrossFade(abilityData.holdReleaseAnimationHash, 0.1f);
				coolTimer.Reset(1f);
				coolTimer.Start();
				break;
			}

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, 100f, buildableLayer))
			{
				Vector3 buildPosition = hit.point;
				buildPosition.y = Mathf.Round(buildPosition.y);

				float wallHeight = wallPrefab.GetComponent<Renderer>().bounds.size.y;
				buildPosition.y = hit.point.y + wallHeight / 2;

				if (currentPreview == null)
				{
					currentPreview = Instantiate(wallPreviewPrefab, buildPosition, Quaternion.identity);
					currentPreview.layer = LayerMask.NameToLayer("PreviewWall");
				}

				canBuild = !IsOverlapping(hit.point);

				if (canBuild)
				{
					lastValidPosition = buildPosition;
					currentPreview.transform.position = buildPosition;
				}
				else
				{
					currentPreview.transform.position = lastValidPosition;
				}

				Renderer previewRenderer = currentPreview.GetComponent<Renderer>();
				if (previewRenderer != null)
				{
					previewRenderer.material.color =
						canBuild ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
				}
			}
		}

		if (currentPreview != null)
		{
			Destroy(currentPreview);
			currentPreview = null;
		}

		yield return new WaitForSeconds(1.15f);

		playerInit.TurnOnCurrentWeaponServerRpc();
	}

	[ServerRpc]
	void RequestSapwnServerRpc(Vector3 targetPos)
	{
		/*GameObject newWall = Instantiate(
				wallPrefab,
				Core.Root.transform.position.With(y: 5),
				Quaternion.identity);*/
		NetworkObject newWall =
			NetworkObjectPool.Singleton.GetNetworkObject(
				wallPrefab,
				Core.Root.transform.position.With(y: 5),
				Quaternion.identity);
		newWall.gameObject.layer = LayerMask.NameToLayer("Wall");
		newWall.GetComponent<ThrowObject>().action += FindDeleteBox;
		if(!newWall.IsSpawned)
			newWall.Spawn();

		newWall.GetComponent<ThrowObject>().TrowInit(newWall.transform, targetPos);

		if (builtWalls.Count >= maxWalls)
			OverflowDeleteBox();

		builtWalls.Enqueue(newWall);
	}

	public void FindDeleteBox(NetworkObject compareObject)
	{
		for (int i = 0; i < builtWalls.Count; i++)
		{
			NetworkObject oldestWall = builtWalls.Dequeue();
			if (oldestWall != compareObject)
			{
				builtWalls.Enqueue(oldestWall);
				continue;
			}

			if (oldestWall.IsSpawned)
			{
				oldestWall.GetComponent<NetworkObject>().Despawn();
				break;
			}
		}
	}

	public void OverflowDeleteBox()
	{
		NetworkObject oldestWall = builtWalls.Dequeue();
		if (oldestWall.IsSpawned)
			oldestWall.GetComponent<NetworkObject>().Despawn();
	}

	bool IsOverlapping(Vector3 position)
	{
		Collider[] wallColliders = Physics.OverlapBox(position,
			wallPrefab.GetComponent<Renderer>().bounds.extents, Quaternion.identity, disbuildableLayer);
		return wallColliders.Length > 0;
	}
}
