using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Architecture.AbilitySystem.Model
{
	public enum AbilityType
	{
		Melee,
		Ranged,
		Projectile,
		AreaOfEffect,
		Buff,
		Debuff
	}

	//Temp UUID(Universally Unique Identifier)
	public struct ScriptableNetworkObject : INetworkSerializable
	{
		public FixedString64Bytes _guid;
		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref _guid);
		}
	}

	[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
	public class AbilityData : ScriptableObject
	{
		public AbilityType abilityType; // Enum으로 타입 분류
		public AnimationClip animationClip;
		public int animationHash;
		public float duration;
		public float exitDuration;
		public bool moveLock;
		public Sprite icon;

		public string Id;

		public bool isHoldAction; // 새로운 필드 추가
		public AnimationClip holdReleaseAnimationClip; // HoldRelease 애니메이션 클립
		public float holdReleaseAnimationDuration;
		public float holdReleaseAnimationExitDuration;
		public int holdReleaseAnimationHash;

		// Projectile 전용 데이터
		public GameObject projectilePrefab;
		public float projectileSpeed;
		public float projectileRange;

		// AreadOfEffect 전용 데이터
		public GameObject effectPrefab;
		public Vector3 startingPoint;

		private void OnValidate()
		{
			animationHash = Animator.StringToHash(animationClip.name);
			if(isHoldAction)
				holdReleaseAnimationHash = Animator.StringToHash(holdReleaseAnimationClip.name);

			if (String.IsNullOrEmpty(Id))
			{
				Id = Guid.NewGuid().ToString();
			}
		}


		// Projectile 전용 데이터 반환
		public (GameObject prefab, float speed, float range) GetProjectileData(AbilityType abilityType)
		{
			if (this.abilityType != abilityType)
			{
				Debug.LogError("AbilityDataSO Type doesn't same!!!");
				return default;
			}
			return (projectilePrefab, projectileSpeed, projectileRange);
		}

		// AreaOfEffect 전용 데이터 반환
		public (GameObject prefab, Vector3 start) GetAreaOfEffectData(AbilityType abilityType)
		{
			if (this.abilityType != abilityType)
			{
				Debug.LogError("AbilityDataSO Type doesn't same!!!");
				return default;
			}
			return (effectPrefab, startingPoint);
		}
	}
}

#if UNITY_EDITOR

namespace Architecture.AbilitySystem.Model
{
	[CustomEditor(typeof(AbilityData))]
	public class AbilityDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			// 대상 ScriptableObject 가져오기
			AbilityData abilityData = (AbilityData)target;

			// Ability Type Enum
			abilityData.abilityType = (AbilityType)EditorGUILayout.EnumPopup("Ability Type", abilityData.abilityType);

			EditorGUILayout.Space(); // 가독성을 위한 간격 추가

			// 공통 데이터
			EditorGUILayout.LabelField("Common Data", EditorStyles.boldLabel);
			abilityData.Id = EditorGUILayout.TextField("Obejct ID", abilityData.Id);
			abilityData.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", abilityData.animationClip, typeof(AnimationClip), false);
			abilityData.animationHash = EditorGUILayout.IntField("AnimHash", abilityData.animationHash);
			abilityData.duration = EditorGUILayout.FloatField("Duration", abilityData.duration);
			abilityData.exitDuration = EditorGUILayout.FloatField("ExitDuration", abilityData.exitDuration);
			abilityData.moveLock = EditorGUILayout.Toggle("Move Lock", abilityData.moveLock);
			abilityData.icon = (Sprite)EditorGUILayout.ObjectField("Icon", abilityData.icon, typeof(Sprite), false);

			//HoldAction 사용여부
			EditorGUILayout.Space();
			abilityData.isHoldAction = EditorGUILayout.Toggle("Is Hold Action", abilityData.isHoldAction);
			if (abilityData.isHoldAction)
			{
				EditorGUILayout.LabelField("Hold Action Data", EditorStyles.boldLabel);
				abilityData.holdReleaseAnimationClip = (AnimationClip)EditorGUILayout.ObjectField("HoldRelease AnimationClip", abilityData.holdReleaseAnimationClip, typeof(AnimationClip), false);
				abilityData.holdReleaseAnimationHash = EditorGUILayout.IntField("HoldRelease AnimationHash", abilityData.holdReleaseAnimationHash);
				abilityData.holdReleaseAnimationDuration = EditorGUILayout.FloatField("HoldRelease AnimationDuration", abilityData.holdReleaseAnimationDuration);
				abilityData.holdReleaseAnimationExitDuration = EditorGUILayout.FloatField("HoldRelease AnimationExitDuration", abilityData.holdReleaseAnimationExitDuration);
			}

			// Ability Type에 따라 다른 필드 표시
			switch (abilityData.abilityType)
			{
				case AbilityType.Projectile:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Projectile Data", EditorStyles.boldLabel);
					EditorGUILayout.HelpBox("Projectile data will go here.", MessageType.Info);
					abilityData.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", abilityData.projectilePrefab, typeof(GameObject), false);
					abilityData.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", abilityData.projectileSpeed);
					abilityData.projectileRange = EditorGUILayout.FloatField("Projectile Range", abilityData.projectileRange);
					break;

				case AbilityType.AreaOfEffect:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("AOE Data", EditorStyles.boldLabel);
					EditorGUILayout.HelpBox("Area of Effect data will go here.", MessageType.Info);
					abilityData.effectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", abilityData.effectPrefab, typeof(GameObject), false);
					abilityData.startingPoint = EditorGUILayout.Vector3Field("Effect Starting Point", abilityData.startingPoint);
					break;

				case AbilityType.Buff:
					break;
				case AbilityType.Debuff:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Buff/Debuff Data", EditorStyles.boldLabel);
					// Buff/Debuff 전용 데이터 추가 예시
					EditorGUILayout.HelpBox("Buff/Debuff specific data goes here.", MessageType.Info);
					break;

				default:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("No Additional Data", EditorStyles.helpBox);
					break;
			}

			// 변경 사항 저장
			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
}
#endif
