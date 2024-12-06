using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChocoOzing.CoreSystem.StatSystem
{
	[Serializable]
	public class Stat
	{
		public event Action<GameObject> OnCurrentValueZero;
		GameObject saveAttacker;
		[field: SerializeField] public float MaxValue { get; private set; }

		public float CurrentValue
		{
			get => currentValue;
			private set
			{
				currentValue = Mathf.Clamp(value, 0f, MaxValue);

				if (currentValue <= 0f)
				{
					Init();
					OnCurrentValueZero?.Invoke(saveAttacker);
				}
			}
		}
		private float currentValue;

		public void Init() => CurrentValue = MaxValue;
		public void Increase(float amount)
		{
			CurrentValue += amount;
		}
		public void Decrease(float amount)
		{
			CurrentValue -= amount;
		}
		public void Decrease(float amount, GameObject attacker)
		{
			saveAttacker = attacker;
			CurrentValue -= amount;
		}
	}
}
