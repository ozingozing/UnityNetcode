using UnityEngine;

namespace Architecture.AbilitySystem.Model
{
	[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
	public class AbilityData : ScriptableObject
	{
		public AnimationClip animtaionClip;
		public int animationHash;
		public float duration;
		public bool moveLock;
		public Sprite icon;

		private void OnValidate()
		{
			animationHash = Animator.StringToHash(animtaionClip.name);
		}
	}
}

