using UnityEngine;

namespace Architecture.AbilitySystem
{
	[CreateAssetMenu(fileName = "AbilityData", menuName = "ScriptableObjects/AbilityData")]
	public class AbilityData : ScriptableObject
	{
		public AnimationClip animtaionClip;
		public int animationHash;
		public float duration;
		public Sprite icon;

		private void OnValidate()
		{
			animationHash = Animator.StringToHash(animtaionClip.name);
		}
	}
}

