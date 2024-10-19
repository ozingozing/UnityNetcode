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
		player.GetComponent<PlayerStats>().Name.OnValueChanged += OnNameChanged;
		player.GetComponent<PlayerStats>().kills.OnValueChanged += OnKillChanged;
		player.GetComponent<PlayerStats>().deaths.OnValueChanged += OnDeathChanged;

		OnKillChanged(0, player.GetComponent<PlayerStats>().kills.Value);
		OnDeathChanged(0, player.GetComponent<PlayerStats>().deaths.Value);
		OnNameChanged("", player.GetComponent<PlayerStats>().Name.Value);

		//transform.GetChild(3).GetComponent<Image>().sprite = LobbyAssets.Instance.GetSprite(player.GetComponent<PlayerStats>().PlayerCharactar);
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

	private void Update()
	{
		if(player != null)
		{
			transform.GetChild(3).GetComponent<Image>().sprite = LobbyAssets.Instance.GetSprite(player.GetComponent<PlayerStats>().PlayerCharactar);
			return;
		}
		/*if(Input.GetKeyDown(KeyCode.U))
		{
			transform.GetChild(3).GetComponent<Image>().sprite = LobbyAssets.Instance.GetSprite(player.GetComponent<PlayerStats>().PlayerCharactar);
		}*/
	}
}
