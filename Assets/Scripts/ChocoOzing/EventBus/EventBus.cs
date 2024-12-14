using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 타입 T를 처리하기 위한 제네릭 Event Bus 구현.
/// 이벤트 등록, 해제 및 발생 기능을 제공합니다.
/// </summary>
/// <typeparam name="T">IEvent 인터페이스를 구현하는 이벤트 타입.</typeparam>
public static class EventBus<T> where T : IEvent
{
	/// <summary>
	/// T 타입의 이벤트를 청취하는 이벤트 바인딩을 저장하는 컬렉션.
	/// </summary>
	static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();

	/// <summary>
	/// 이벤트 바인딩을 등록하여 T 타입의 이벤트를 청취할 수 있도록 설정.
	/// </summary>
	/// <param name="binding">등록할 이벤트 바인딩.</param>
	public static void Register(EventBinding<T> binding) => bindings.Add(binding);

	/// <summary>
	/// 이벤트 바인딩을 해제하여 더 이상 이벤트를 청취하지 않도록 설정.
	/// </summary>
	/// <param name="binding">해제할 이벤트 바인딩.</param>
	public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

	/// <summary>
	/// T 타입의 이벤트를 발생시키고, 등록된 모든 바인딩을 호출.
	/// </summary>
	/// <param name="event">발생시킬 이벤트.</param>
	public static void Raise(T @event)
	{
		foreach (var binding in bindings)
		{
			binding.OnEvent.Invoke(@event);    // 인자를 포함하는 이벤트 호출
			binding.OnEventNoArgs.Invoke();   // 인자 없는 이벤트 호출
		}
	}

	static void Clear()
	{
		Debug.Log($"Clearing {typeof(T).Name} bindings");
		bindings.Clear();
	}
}