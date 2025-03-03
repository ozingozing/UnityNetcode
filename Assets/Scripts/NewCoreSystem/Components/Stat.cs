using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChocoOzing.CoreSystem.StatSystem
{
	[Serializable]
	public class Stat
	{
		//ServerOnly
		public event Action<GameObject> OnCurrentValueZero;
		GameObject saveAttacker;
		[field: SerializeField] public float MaxValue { get; private set; }

		public float CurrentValue
		{
			get => currentValue;
			private set
			{
				//currentValue = Mathf.Clamp(value, 0f, MaxValue);

				//if (currentValue <= 0f)
				//{
				//	OnCurrentValueZero?.Invoke(saveAttacker);
				//	Init();
				//}
			}
		}
		private float currentValue;

		public void Init()
		{
			//GameObject.Find("Hp").GetComponent<Image>().fillAmount = 1;
			//CurrentValue = MaxValue;
		}

		public float Decrease(float amount, GameObject attacker)
		{
			//saveAttacker = attacker;
			//Decrease(amount);
			return CurrentValue;
		}
		
		public void Increase(float amount)
		{
			//CurrentValue += amount;
		}
		
		public void Decrease(float amount)
		{
			//CurrentValue -= amount;
		}

		public void DecreaseBroadcast(float currentHp, bool isLocalPlayer)
		{
			//if(isLocalPlayer)
			//{
			//	GameObject.Find("Hp").GetComponent<Image>().fillAmount = Mathf.Clamp01(currentHp / MaxValue);
			//}
		}
	}
}
