using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Services.Authentication;
using static LobbyManager;
using Unity.Netcode;
using System.Linq;
using Unity.Collections;
using System;

public class PlayerKDA : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI NameUI;
	[SerializeField]
	private TextMeshProUGUI ScoreUI;

	int kill;
	int death;
	public GameObject player;
	public void TracePlayer(GameObject player)
	{
		this.player = player;
		player.GetComponent<PlayerInit>().Name.OnValueChanged += OnNameChanged;
		player.GetComponent<PlayerInit>().kills.OnValueChanged += OnKillChanged;
		player.GetComponent<PlayerInit>().deaths.OnValueChanged += OnDeathChanged;

		OnKillChanged(0, player.GetComponent<PlayerInit>().kills.Value);
		OnDeathChanged(0, player.GetComponent<PlayerInit>().deaths.Value);
		OnNameChanged("", player.GetComponent<PlayerInit>().Name.Value);

		transform.GetChild(3).GetComponent<Image>().sprite = LobbyAssets.Instance.GetSprite(player.GetComponent<PlayerInit>().PlayerCharactar);
	}

	private void OnDeathChanged(int previousValue, int newValue)
	{
		death = newValue;
		SetKD();
	}

	private void OnKillChanged(int previousValue, int newValue)
	{
		kill = newValue;
		SetKD();
	}

	private void SetKD()
	{
		ScoreUI.text = $"K/{kill} D/{death}";
	}

	private void OnNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
	{
		NameUI.text = newValue.ToString();	
	}

}
