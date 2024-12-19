using System;

namespace ChocoOzing.EventBusSystem
{
	internal interface IEventBinding<T>
	{
		public Action<T> OnEvent { get; protected set; }
		public Action OnEventNoArgs { get; protected set; }
	}

	public class EventBinding<T> : IEventBinding<T> where T : IEvent
	{
		private Action<T> onEvent = _ => { };
		private Action onEventNoArgs = () => { };

		Action<T> IEventBinding<T>.OnEvent { get => onEvent; set => onEvent = value; }
		Action IEventBinding<T>.OnEventNoArgs { get => onEventNoArgs; set => onEventNoArgs = value; }

		public EventBinding(Action<T> onEvent) => this.onEvent = onEvent;
		public EventBinding(Action onEvnet) => this.onEventNoArgs = onEvnet;

		public void Add(Action onEvent) => onEventNoArgs += onEvent;
		public void Remove(Action onEvent) => onEventNoArgs -= onEvent;

		public void Add(Action<T> onEvent) => this.onEvent += onEvent;
		public void Remove(Action<T> onEvent) => this.onEvent -= onEvent;
	}
}