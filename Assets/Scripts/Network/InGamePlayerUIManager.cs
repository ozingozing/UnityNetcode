using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using System.Linq;
using Unity.Services.Authentication;
using static LobbyManager;

public class InGamePlayerUIManager : MonoBehaviour
{
	public static InGamePlayerUIManager Instance { get; private set; }
	
	[SerializeField] private Transform playerSingleTemplate;
	[SerializeField] private Transform container;
	private RectTransform rectTransform;


	private void Awake()
	{
		Instance = this;
		rectTransform = GetComponent<RectTransform>();
	}

	private void OnEnable()
	{
		PlayerStats.OnPlayerSpawn += OnplayerSpanwed;
	}

	private void OnDisable()
	{
		PlayerStats.OnPlayerSpawn -= OnplayerSpanwed;
	}

	private void OnplayerSpanwed(GameObject player)
	{
		Transform PlayerUI = Instantiate(playerSingleTemplate, container);
		PlayerUI.GetComponent<PlayerKDA>().TracePlayer(player);
	}

	public void PanelFadeIn()
	{
		rectTransform.transform.localPosition = new Vector3(500, 0, 0);
		rectTransform.DOAnchorPos(new Vector2(0, 0), 0.5f, false).SetEase(Ease.OutElastic);
	}

	public void PanelFadeOut()
	{
		rectTransform.transform.localPosition = new Vector3(0, 0, 0);
		rectTransform.DOAnchorPos(new Vector2(500, 0), 0.5f, false).SetEase(Ease.InOutQuint);
	}
}
