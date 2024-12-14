using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �̺�Ʈ Ÿ�� T�� ó���ϱ� ���� ���׸� Event Bus ����.
/// �̺�Ʈ ���, ���� �� �߻� ����� �����մϴ�.
/// </summary>
/// <typeparam name="T">IEvent �������̽��� �����ϴ� �̺�Ʈ Ÿ��.</typeparam>
public static class EventBus<T> where T : IEvent
{
	/// <summary>
	/// T Ÿ���� �̺�Ʈ�� û���ϴ� �̺�Ʈ ���ε��� �����ϴ� �÷���.
	/// </summary>
	static readonly HashSet<IEventBinding<T>> bindings = new HashSet<IEventBinding<T>>();

	/// <summary>
	/// �̺�Ʈ ���ε��� ����Ͽ� T Ÿ���� �̺�Ʈ�� û���� �� �ֵ��� ����.
	/// </summary>
	/// <param name="binding">����� �̺�Ʈ ���ε�.</param>
	public static void Register(EventBinding<T> binding) => bindings.Add(binding);

	/// <summary>
	/// �̺�Ʈ ���ε��� �����Ͽ� �� �̻� �̺�Ʈ�� û������ �ʵ��� ����.
	/// </summary>
	/// <param name="binding">������ �̺�Ʈ ���ε�.</param>
	public static void Deregister(EventBinding<T> binding) => bindings.Remove(binding);

	/// <summary>
	/// T Ÿ���� �̺�Ʈ�� �߻���Ű��, ��ϵ� ��� ���ε��� ȣ��.
	/// </summary>
	/// <param name="event">�߻���ų �̺�Ʈ.</param>
	public static void Raise(T @event)
	{
		foreach (var binding in bindings)
		{
			binding.OnEvent.Invoke(@event);    // ���ڸ� �����ϴ� �̺�Ʈ ȣ��
			binding.OnEventNoArgs.Invoke();   // ���� ���� �̺�Ʈ ȣ��
		}
	}

	static void Clear()
	{
		Debug.Log($"Clearing {typeof(T).Name} bindings");
		bindings.Clear();
	}
}