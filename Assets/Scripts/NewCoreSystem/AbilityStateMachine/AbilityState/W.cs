using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using Mono.CSharp;
using System.Collections;
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
				player.Anim.CrossFade(abilityData.holdReleaseAnimationHash, 0.1f);
				coolTimer.Reset(1f);
				coolTimer.Start();
				break;
			}
		}
		yield return new WaitForSeconds(1.15f);
		playerInit.TurnOnCurrentWeaponServerRpc();
	}
}
