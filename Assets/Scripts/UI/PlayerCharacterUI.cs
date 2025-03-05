using ChocoOzing.CoreSystem.StatSystem;
using ChocoOzing.EventBusSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : MonoBehaviour
{
	private void Awake()
	{
		EventBus<PlayerOnSpawnState>.Register(new EventBinding<PlayerOnSpawnState>(SetPlayerImage));
	}

	public void SetPlayerImage(PlayerOnSpawnState player)
	{
		if(player.player.GetComponent<PlayerInit>().IsLocalPlayer)
			GetComponent<Image>().sprite = LobbyAssets.Instance.GetSprite(player.player.GetComponent<PlayerInit>().PlayerCharactar);
	}
}
