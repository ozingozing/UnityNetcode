using Architecture.AbilitySystem.Model;
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
				if(i >= keys.Length)
				{
					Debug.LogError("Not enough keycodes for the number of buttons.");
				}

				buttons[i].Initialize(i, keys[i]);
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
