using ChocoOzing.CommandSystem;

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
		
		public Ability(AbilityData data, IEntity player)
		{
			this.data = data;
			command = new AbilityCommand(data, player);
		}

		public AbilityCommand GetCommand() => command;
	}
}