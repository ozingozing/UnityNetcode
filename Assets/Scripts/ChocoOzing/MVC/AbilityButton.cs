using System;
using UnityEngine;
using UnityEngine.UI;

namespace Architecture.AbilitySystem.View
{
	public class AbilityButton : MonoBehaviour
	{
		public Image radialImage;
		public Image abilitIcon;
		public int index;
		public KeyCode key;

		public event Action<int> OnButtonPressed = delegate { };

		private void Start()
		{
			GetComponent<Button>().onClick.AddListener(() => OnButtonPressed(index));
		}

		private void Update()
		{
			if(Input.GetKeyDown(key))
				OnButtonPressed(index);
		}

		public void RegisterListener(Action<int> listener)
		{
			OnButtonPressed += listener;
		}

		public void DeRegisterListenr(Action<int> listener)
		{
			OnButtonPressed -= listener;
		}

		public void Initialize(int index, KeyCode key)
		{
			this.index = index;
			this.key = key;
		}

		public void UpdateButtonSprite(Sprite newIcon)
		{
			abilitIcon.sprite = newIcon;
		}

		public void UpdateRedialFill(float progress)
		{
			if(radialImage)
			{
				radialImage.fillAmount = progress;
			}
		}
	}
}
