using ChocoOzing.CoreSystem.StatSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    Image Hpbar;

	private void Start()
	{
		Hpbar = GetComponent<Image>();
	}

	float MaxValue;
	public void AddListener(Stats playerStats)
	{
		if(playerStats.IsLocalPlayer)
		{
			MaxValue = playerStats.MaxValue;
			playerStats.playerHp.AddListener(SetHpbar);
		}
	}

	void SetHpbar(float hp)
	{
		Hpbar.fillAmount = Mathf.Clamp01(hp / MaxValue);
	}
}
