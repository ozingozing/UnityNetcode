using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Utilities;
using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
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
	private Queue<GameObject> builtWalls = new Queue<GameObject>();

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
				BuildWall();
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

	void BuildWall()
	{
		if (currentPreview != null)
		{
			/*GameObject newWall = Instantiate(wallPrefab,
				currentPreview.transform.position , Quaternion.identity);*/
			GameObject newWall = Instantiate(wallPrefab,
				Core.Root.transform.position.With(y: 2.3f), Quaternion.identity);
			newWall.layer = LayerMask.NameToLayer("Wall");

			newWall.GetComponent<ThrowObject>().TrowInit(newWall.transform, currentPreview.transform);

			if (builtWalls.Count >= maxWalls)
			{
				GameObject oldestWall = builtWalls.Dequeue();
				Destroy(oldestWall);
			}

			builtWalls.Enqueue(newWall);
		}
	}

	bool IsOverlapping(Vector3 position)
	{
		Collider[] wallColliders = Physics.OverlapBox(position,
			wallPrefab.GetComponent<Renderer>().bounds.extents, Quaternion.identity, disbuildableLayer);
		return wallColliders.Length > 0;
	}
}
