using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ChocoOzing.CoreSystem.StatSystem
{
	public class Stats : CoreComponent
	{
		//[field: SerializeField] public Stat Health { get; private set; }
		//[field: SerializeField] public Stat Poise { get; private set; }

		//[SerializeField] private float poiseRecoverRate;

		public event Action<GameObject> OnCurrentValueZero;
		public Observer<float> playerHp = new Observer<float>(100);
		[field: SerializeField] public float MaxValue { get; private set; }
		public float CurrentValue;
		GameObject saveAttacker;

		public override void OnNetworkSpawn()
		{
			MaxValue = 100;
			CurrentValue = MaxValue; 
			GameObject.Find("Hp").GetComponent<PlayerHpUI>().AddListener(this);
			playerHp.AddListener(PlayerHpChanged);

			base.OnNetworkSpawn();
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public void Init() => playerHp.Set(MaxValue);
		void PlayerHpChanged(float newValue)
		{
			CurrentValue = Mathf.Clamp(newValue, 0f, MaxValue);
			if (CurrentValue <= 0f)
			{
				OnCurrentValueZero?.Invoke(saveAttacker);
				StartCoroutine(InitPlayerHp());
			}
		}

		IEnumerator InitPlayerHp()
		{
			yield return new WaitForSeconds(1f);
			Init();
		}

		public float Decrease(float amount, GameObject attacker)
		{
			saveAttacker = attacker;
			playerHp.Value -= amount;
			Debug.Log(playerHp.Value);
			return playerHp.Value;
		}
		public void DecreaseBroadcast(float currentHp, bool isLocalPlayer)
		{
			if (isLocalPlayer)
			{
				playerHp.Set(currentHp);
			}
		}
	}
}
