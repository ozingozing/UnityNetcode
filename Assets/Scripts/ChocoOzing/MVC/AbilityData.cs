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
		public AbilityType abilityType; // Enum���� Ÿ�� �з�
		public AnimationClip animationClip;
		public int animationHash;
		public float duration;
		public bool moveLock;
		public Sprite icon;

		public string Id;

		// Projectile ���� ������
		public GameObject projectilePrefab;
		public float projectileSpeed;
		public float projectileRange;

		// AreadOfEffect ���� ������
		public GameObject effectPrefab;
		public GameObject particlePrefab;
		public Vector3 startingPoint;

		private void OnValidate()
		{
			animationHash = Animator.StringToHash(animationClip.name);

			if (String.IsNullOrEmpty(Id))
			{
				Id = Guid.NewGuid().ToString();
			}
		}


		// Projectile ���� ������ ��ȯ
		public (GameObject prefab, float speed, float range) GetProjectileData(AbilityType abilityType)
		{
			if (this.abilityType != abilityType)
			{
				Debug.LogError("AbilityDataSO Type doesn't same!!!");
				return default;
			}
			return (projectilePrefab, projectileSpeed, projectileRange);
		}

		// AreaOfEffect ���� ������ ��ȯ
		public (GameObject prefab, Vector3 start, GameObject particle) GetAreaOfEffectData(AbilityType abilityType)
		{
			if (this.abilityType != abilityType)
			{
				Debug.LogError("AbilityDataSO Type doesn't same!!!");
				return default;
			}
			return (effectPrefab, startingPoint, particlePrefab);
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
			// ��� ScriptableObject ��������
			AbilityData abilityData = (AbilityData)target;

			// Ability Type Enum
			abilityData.abilityType = (AbilityType)EditorGUILayout.EnumPopup("Ability Type", abilityData.abilityType);

			EditorGUILayout.Space(); // �������� ���� ���� �߰�

			// ���� ������
			EditorGUILayout.LabelField("Common Data", EditorStyles.boldLabel);
			abilityData.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", abilityData.animationClip, typeof(AnimationClip), false);
			abilityData.animationHash = EditorGUILayout.IntField("AnimHash", abilityData.animationHash);
			abilityData.duration = EditorGUILayout.FloatField("Duration", abilityData.duration);
			abilityData.moveLock = EditorGUILayout.Toggle("Move Lock", abilityData.moveLock);
			abilityData.icon = (Sprite)EditorGUILayout.ObjectField("Icon", abilityData.icon, typeof(Sprite), false);
			
			// Ability Type�� ���� �ٸ� �ʵ� ǥ��
			switch (abilityData.abilityType)
			{
				case AbilityType.Projectile:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Projectile Data", EditorStyles.boldLabel);
					abilityData.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", abilityData.projectilePrefab, typeof(GameObject), false);
					abilityData.projectileSpeed = EditorGUILayout.FloatField("Projectile Speed", abilityData.projectileSpeed);
					abilityData.projectileRange = EditorGUILayout.FloatField("Projectile Range", abilityData.projectileRange);
					break;

				case AbilityType.AreaOfEffect:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("AOE Data", EditorStyles.boldLabel);
					// AOE ���� ������ �߰� ����
					EditorGUILayout.HelpBox("Area of Effect data will go here.", MessageType.Info);
					abilityData.effectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", abilityData.effectPrefab, typeof(GameObject), false);
					abilityData.startingPoint = EditorGUILayout.Vector3Field("Effect Starting Point", abilityData.startingPoint);
					abilityData.particlePrefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab", abilityData.particlePrefab, typeof(GameObject), false);
					abilityData.Id = EditorGUILayout.TextField("Obejct ID", abilityData.Id);
					break;

				case AbilityType.Buff:
				case AbilityType.Debuff:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Buff/Debuff Data", EditorStyles.boldLabel);
					// Buff/Debuff ���� ������ �߰� ����
					EditorGUILayout.HelpBox("Buff/Debuff specific data goes here.", MessageType.Info);
					break;

				default:
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("No Additional Data", EditorStyles.helpBox);
					break;
			}

			// ���� ���� ����
			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
}
#endif
