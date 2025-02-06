using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class W : CoreComponent, ISkillAction
{
	public AbilityButton myButton;
	public AbilityData abilityData
	{
		get => AbilityData;
		set => AbilityData = value;
	}
	[SerializeField] private AbilityData AbilityData;

	public bool isHoldAction {
		get => IsHoldAction;
		set => IsHoldAction = value;
	}
	[SerializeField] private bool IsHoldAction = true;

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

	public void SetAbilityData(AbilityData abilityData, AbilityButton myButton)
	{
		this.myButton = myButton;
		this.abilityData = abilityData;
		player = Core.Root.GetComponent<MyPlayer>();
		coolTimer = Core.GetCoreComponent<AbilitySystem>().controller.cooltimer;
	}

	public void Action(PlayerAnimationEvent @evnet)
	{
		if (IsLocalPlayer)
		{
			isHoldAction = abilityData.isHoldAction ? true : false;
			StartCoroutine(MyAction());
		}
	}

	IEnumerator MyAction()
	{
		player.GunStateMachine.CurrentState.isAiming.Set(true);
		while (true)
		{
			if (coolTimer.IsRunning)
				coolTimer.Pause();

			yield return null;
			
			//Action Next
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				isHoldAction = false;
				if(currentPreview != null)
					RequestSapwnServerRpc(currentPreview.transform.position);
				player.AnimationManager.AnimPlay(abilityData.holdReleaseAnimationHash);
				player.PlayerAbilityState.setDefualtDuration = abilityData.holdReleaseAnimationExitDuration;
				player.GunStateMachine.CurrentState.isAiming.Set(false);
				break;
			}
			//Cancle Action
			else if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(myButton.key))
			{
				isHoldAction = false;
				player.GunStateMachine.CurrentState.isAiming.Set(false);
				break;
			}

			Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
			Ray ray = Camera.main.ScreenPointToRay(screenCenter);
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

		coolTimer.Reset(player.PlayerAbilityState.setDefualtDuration);
		coolTimer.Start();
	}

	[ServerRpc]
	void RequestSapwnServerRpc(Vector3 targetPos)
	{
		NetworkObject newWall =
			NetworkObjectPool.Singleton.GetNetworkObject(
				abilityData.GetProjectileData(abilityData.abilityType).prefab,
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
			ThrowObject throwObject = oldestWall.GetComponent<ThrowObject>();
			if (oldestWall != compareObject)
			{
				builtWalls.Enqueue(oldestWall);
				continue;
			}

			if (oldestWall.IsSpawned)
			{
				oldestWall.Despawn();
				throwObject.action -= FindDeleteBox;
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
			wallPrefab.GetComponent<Renderer>().bounds.extents * .95f, Quaternion.identity, disbuildableLayer);
		return wallColliders.Length > 0;
	}
}
