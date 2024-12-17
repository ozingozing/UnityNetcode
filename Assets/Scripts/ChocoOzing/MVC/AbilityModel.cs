using Architecture.AbilitySystem.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architecture.AbilitySystem.Model
{
	public class AbilityModel
	{
		public readonly ObservableList<Ability> abilities = new();
		
		public void Add(Ability a)
		{
			abilities.Add(a);
		}
	}

	public class Ability
	{
		AbilityCommand command;
		public readonly AbilityData data;
		
		public Ability(AbilityData data)
		{
			this.data = data;
			command = new AbilityCommand(data);
		}

		public AbilityCommand CreateCommand() => command;
	}
}