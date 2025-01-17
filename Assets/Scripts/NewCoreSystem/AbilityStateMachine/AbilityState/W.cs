using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using UnityEngine;

public class W : CoreComponent, ISkillAction
{
	public AbilityData abilityData { get => AbilityData; set => AbilityData = value; }
	[SerializeField] private AbilityData AbilityData;
	MyPlayer player;
	public void SetAbilityData(AbilityData abilityData)
	{
		this.abilityData = abilityData;
		player = Core.Root.GetComponent<MyPlayer>();	
	}

	public void Action(PlayerAnimationEvent @evnet)
	{
	}

}
