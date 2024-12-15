using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ChocoOzing.EventBusSystem
{
	/// <summary>
	/// Event Bus 유틸리티 클래스. 이벤트 타입과 관련된 Event Bus 초기화, 정리 등의 기능을 제공합니다.
	/// </summary>
	public static class EventBusUtil
	{
		/// <summary>
		/// 모든 IEvent 타입의 읽기 전용 리스트.
		/// </summary>
		public static IReadOnlyList<Type> EventTypes { get; set; }

		/// <summary>
		/// 모든 EventBus 타입의 읽기 전용 리스트.
		/// </summary>
		public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
		/// <summary>
		/// 현재 플레이 모드 상태.
		/// </summary>
		public static PlayModeStateChange PlayModeState { get; set; }

		/// <summary>
		/// 에디터 환경에서 초기화 메서드. Unity 에디터의 플레이 모드 상태 변경 이벤트를 등록합니다.
		/// </summary>
		[InitializeOnLoadMethod]
		public static void InitializeEditor()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		/// <summary>
		/// 플레이 모드 상태 변경 이벤트 핸들러.
		/// 플레이 모드가 종료될 때 모든 Event Bus를 정리합니다.
		/// </summary>
		/// <param name="state">플레이 모드 상태 변경 값.</param>
		static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			PlayModeState = state;
			if (state == PlayModeStateChange.ExitingPlayMode)
				ClearAllBuses();
		}
#endif

		/// <summary>
		/// 런타임 초기화 메서드. 모든 IEvent 타입과 관련된 EventBus를 초기화합니다.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
			EventBusTypes = InitailizeAllBuses();
		}

		/// <summary>
		/// 모든 EventBus를 초기화하고 초기화된 EventBus 타입을 반환합니다.
		/// </summary>
		/// <returns>초기화된 EventBus 타입 리스트.</returns>
		private static List<Type> InitailizeAllBuses()
		{
			List<Type> eventBusTypes = new List<Type>();

			// EventBus 제네릭 타입 정의를 가져옵니다.
			var typedef = typeof(EventBus<>);

			// IEvent를 구현하는 모든 타입(EventTypes)에 대해 반복
			foreach (var eventType in EventTypes)
			{
				// MakeGenericType: 제네릭 타입 정의(typedef)에 특정 타입(eventType)을 넣어
				// 구체적인 제네릭 타입을 생성합니다.
				// 예: EventBus<>를 EventBus<MyEvent>로 변환
				var busType = typedef.MakeGenericType(eventType);

				// 초기화된 EventBus 타입을 리스트에 추가
				eventBusTypes.Add(busType);

				// 디버그 로그를 통해 초기화된 EventBus 타입 확인
				Debug.Log($"Initialized EventBus<{eventType.Name}>");
			}

			return eventBusTypes;
		}

		/// <summary>
		/// 모든 EventBus를 정리합니다.
		/// </summary>
		public static void ClearAllBuses()
		{
			Debug.Log("Clearing all buses...");

			// IEvent를 구현하는 모든 타입(EventTypes)에 대해 반복
			for (int i = 0; i < EventBusTypes.Count; i++)
			{
				var busType = EventBusTypes[i];

				// GetMethod: 특정 메서드 이름("Clear")을 사용해
				// 정적 비공개 메서드(static, non-public)를 리플렉션으로 가져옵니다.
				// 이 메서드는 EventBus 내부에서 선언된 Clear 메서드를 의미합니다.
				var clearMethod = busType.GetMethod(
					"Clear",
					BindingFlags.Static // 정적 메서드
					| BindingFlags.NonPublic // 비공개 메서드
				);

				// Invoke: 리플렉션으로 가져온 Clear 메서드를 실행합니다.
				// 여기서는 정적 메서드이므로 첫 번째 매개변수는 null,
				// 두 번째 매개변수는 해당 메서드에 전달할 인자입니다(여기선 없음).
				clearMethod.Invoke(null, null);
			}
		}
	}
}
