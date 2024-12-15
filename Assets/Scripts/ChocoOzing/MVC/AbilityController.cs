using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
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
		readonly CountdownTimer timer = new CountdownTimer(0);

		AbilityController(AbilityView view, AbilityModel model)
		{
			this.view = view;
			this.model = model;

			ConnectModel();
			ConnectView();
		}

		public void Update(float deltaTime)
		{
			timer.Tick(deltaTime);
			view.UpdateRedial(timer.Progress);

			if(!timer.IsRunning && abilityQueue.TryDequeue(out AbilityCommand cmd))
			{
				cmd.Execute();
				timer.Reset(cmd.duration);
				timer.Start();
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
			if(timer.Progress < 0.25f || !timer.IsRunning)
			{
				if (model.abilities[index] != null)
				{
					abilityQueue.Enqueue(model.abilities[index].CreateCommand());
				}
			}
			EventSystem.current.SetSelectedGameObject(null);
		}

		void ConnectModel()
		{
			model.abilities.AnyValueChanged += UpdateButtons;
		}

		private void UpdateButtons(IList<Ability> updatedAbilities) => view.UpdateButtonSprites(updatedAbilities);


		public class Builder
		{
			readonly AbilityModel model = new AbilityModel();

			public Builder WithAbilities(AbilityData[] datas)
			{
				foreach(var data in datas)
				{
					model.Add(new Ability(data));
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