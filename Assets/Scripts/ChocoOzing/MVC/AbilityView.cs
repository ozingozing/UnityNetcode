using Architecture.AbilitySystem.Model;
using ChocoOzing.EventBusSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.AbilitySystem.View
{
	public class AbilityView : MonoBehaviour
	{
		[SerializeField] public AbilityButton[] buttons;
		readonly KeyCode[] keys = { KeyCode.Y, KeyCode.T, KeyCode.F, KeyCode.D, KeyCode.G };

		private void Awake()
		{
			for (int i = 0; i < buttons.Length; i++)
			{
				if (i >= keys.Length)
				{
					Debug.LogError("Not enough keycodes for the number of buttons.");
				}

				buttons[i].Initialize(i, keys[i]);
			}
		}

		EventBinding<LobbyEventArgs> eventBinding;
		private void Start()
		{
			eventBinding = new EventBinding<LobbyEventArgs>(LobbyEvent);
			EventBus<LobbyEventArgs>.Register(eventBinding);
			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			eventBinding.Remove(LobbyEvent);
			EventBus<LobbyEventArgs>.Deregister(eventBinding);
			eventBinding = null;
		}

		private void LobbyEvent(LobbyEventArgs lobby)
		{
			switch (lobby.state)
			{
				case LobbyState.StartGame:
					gameObject.SetActive(true);
					break;
				case LobbyState.LeaveGame:
					gameObject.SetActive(false);
					break;
				default:
					break;
			}
		}

		public void UpdateRedial(float progress)
		{
			if (float.IsNaN(progress))
				progress = 0;
			Array.ForEach(buttons, button => button.UpdateRedialFill(progress));
		}

		public void UpdateButtonSprites(IList<Ability> abilityes)
		{
			for(int i = 0; i < buttons.Length; i++)
			{
				if(i < abilityes.Count)
				{
					buttons[i].UpdateButtonSprite(abilityes[i].data.icon);
				}
				else
				{
					buttons[i].gameObject.SetActive(false);
				}
			}
		}
	}
}
