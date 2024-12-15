using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChocoOzing.EventBusSystem
{
	/// <summary>
	/// Event Bus ��ƿ��Ƽ Ŭ����. �̺�Ʈ Ÿ�԰� ���õ� Event Bus �ʱ�ȭ, ���� ���� ����� �����մϴ�.
	/// </summary>
	public static class EventBusUtil
	{
		/// <summary>
		/// ��� IEvent Ÿ���� �б� ���� ����Ʈ.
		/// </summary>
		public static IReadOnlyList<Type> EventTypes { get; set; }

		/// <summary>
		/// ��� EventBus Ÿ���� �б� ���� ����Ʈ.
		/// </summary>
		public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
		/// <summary>
		/// ���� �÷��� ��� ����.
		/// </summary>
		public static PlayModeStateChange PlayModeState { get; set; }

		/// <summary>
		/// ������ ȯ�濡�� �ʱ�ȭ �޼���. Unity �������� �÷��� ��� ���� ���� �̺�Ʈ�� ����մϴ�.
		/// </summary>
		[InitializeOnLoadMethod]
		public static void InitializeEditor()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		/// <summary>
		/// �÷��� ��� ���� ���� �̺�Ʈ �ڵ鷯.
		/// �÷��� ��尡 ����� �� ��� Event Bus�� �����մϴ�.
		/// </summary>
		/// <param name="state">�÷��� ��� ���� ���� ��.</param>
		static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			PlayModeState = state;
			if (state == PlayModeStateChange.ExitingPlayMode)
				ClearAllBuses();
		}
#endif

		/// <summary>
		/// ��Ÿ�� �ʱ�ȭ �޼���. ��� IEvent Ÿ�԰� ���õ� EventBus�� �ʱ�ȭ�մϴ�.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
			EventBusTypes = InitailizeAllBuses();
		}

		/// <summary>
		/// ��� EventBus�� �ʱ�ȭ�ϰ� �ʱ�ȭ�� EventBus Ÿ���� ��ȯ�մϴ�.
		/// </summary>
		/// <returns>�ʱ�ȭ�� EventBus Ÿ�� ����Ʈ.</returns>
		private static List<Type> InitailizeAllBuses()
		{
			List<Type> eventBusTypes = new List<Type>();

			// EventBus ���׸� Ÿ�� ���Ǹ� �����ɴϴ�.
			var typedef = typeof(EventBus<>);

			// IEvent�� �����ϴ� ��� Ÿ��(EventTypes)�� ���� �ݺ�
			foreach (var eventType in EventTypes)
			{
				// MakeGenericType: ���׸� Ÿ�� ����(typedef)�� Ư�� Ÿ��(eventType)�� �־�
				// ��ü���� ���׸� Ÿ���� �����մϴ�.
				// ��: EventBus<>�� EventBus<MyEvent>�� ��ȯ
				var busType = typedef.MakeGenericType(eventType);

				// �ʱ�ȭ�� EventBus Ÿ���� ����Ʈ�� �߰�
				eventBusTypes.Add(busType);

				// ����� �α׸� ���� �ʱ�ȭ�� EventBus Ÿ�� Ȯ��
				Debug.Log($"Initialized EventBus<{eventType.Name}>");
			}

			return eventBusTypes;
		}

		/// <summary>
		/// ��� EventBus�� �����մϴ�.
		/// </summary>
		public static void ClearAllBuses()
		{
			Debug.Log("Clearing all buses...");

			// IEvent�� �����ϴ� ��� Ÿ��(EventTypes)�� ���� �ݺ�
			for (int i = 0; i < EventBusTypes.Count; i++)
			{
				var busType = EventBusTypes[i];

				// GetMethod: Ư�� �޼��� �̸�("Clear")�� �����
				// ���� ����� �޼���(static, non-public)�� ���÷������� �����ɴϴ�.
				// �� �޼���� EventBus ���ο��� ����� Clear �޼��带 �ǹ��մϴ�.
				var clearMethod = busType.GetMethod(
					"Clear",
					BindingFlags.Static // ���� �޼���
					| BindingFlags.NonPublic // ����� �޼���
				);

				// Invoke: ���÷������� ������ Clear �޼��带 �����մϴ�.
				// ���⼭�� ���� �޼����̹Ƿ� ù ��° �Ű������� null,
				// �� ��° �Ű������� �ش� �޼��忡 ������ �����Դϴ�(���⼱ ����).
				clearMethod.Invoke(null, null);
			}
		}
	}
}
