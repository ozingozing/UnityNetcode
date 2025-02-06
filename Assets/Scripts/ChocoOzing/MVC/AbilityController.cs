using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.CommandSystem;
using ChocoOzing.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architecture.AbilitySystem.Controller
{
	public class AbilityController
	{
		readonly AbilityModel model;
		readonly AbilityView view;
		readonly Queue<AbilityCommand> abilityQueue = new Queue<AbilityCommand>();
		public readonly CountdownTimer cooltimer = new CountdownTimer(0);

		AbilityController(AbilityView view, AbilityModel model)
		{
			this.view = view;
			this.model = model;

			ConnectModel();
			ConnectView();
		}

		public void Update(float deltaTime)
		{
			cooltimer.Tick(deltaTime);
			view.UpdateRedial(cooltimer.Progress);

			if(!cooltimer.IsRunning && abilityQueue.TryDequeue(out AbilityCommand cmd))
			{
				cmd.Execute();
				cooltimer.Reset(cmd.duration);
				cooltimer.Start();
			}
		}

		void ConnectView()
		{
			for(int i = 0; i < view.buttons.Length; i++)
			{
				view.buttons[i].RegisterListener(OnAbilityButtonPressed);
			}
			view.UpdateButtonSprites(model.abilities);
		}

		void OnAbilityButtonPressed(int index)
		{
			if(cooltimer.IsFinished)
			{
				if (model.abilities[index] != null)
				{
					abilityQueue.Enqueue(model.abilities[index].GetCommand());
				}
			}
			EventSystem.current.SetSelectedGameObject(null);
		}

		void ConnectModel()
		{
			model.abilities.AnyValueChanged += UpdateButtons;
		}

		private void UpdateButtons(IList<Ability> updatedAbilities) => view.UpdateButtonSprites(updatedAbilities);

		public void Clear()
		{
			for (int i = 0; i < view.buttons.Length; i++)
			{
				view.buttons[i].DeRegisterListenr(OnAbilityButtonPressed);
			}
			model.abilities.Clear();
		}

		public class Builder
		{
			readonly AbilityModel model = new AbilityModel();

			public Builder WithAbilities(AbilityData[] datas, ulong ownerClientId)
			{
				foreach(var data in datas)
				{
					model.Add(new Ability(data, ownerClientId));
				}
				return this;
			}

			public AbilityController Build(AbilityView view)
			{
				if (view == null) Debug.LogError("View Error");
				return new AbilityController(view, model);
			}
		}
	}
}