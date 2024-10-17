using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class InGamePlayerUIManager : MonoBehaviour
{
	public static InGamePlayerUIManager Instance { get; private set; }
	
	[SerializeField] private Transform playerSingleTemplate;
	[SerializeField] private Transform container;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		NetworkPlayer.OnPlayerSpawn += OnplayerSpanwed;
	}

	private void OnDisable()
	{
		NetworkPlayer.OnPlayerSpawn -= OnplayerSpanwed;
	}

	private void OnplayerSpanwed(GameObject player)
	{
		Transform PlayerUI = Instantiate(playerSingleTemplate, container);
		PlayerUI.GetComponent<PlayerKDA>().TracePlayer(player);
	}
}
