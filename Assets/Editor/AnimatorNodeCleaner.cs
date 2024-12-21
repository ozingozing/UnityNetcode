using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorNodeCleaner : MonoBehaviour
{
	[MenuItem("Tools/Clean Animator Nodes")]
	static void CleanAnimatorNodes()
	{
		AnimatorController animator = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Invector-3rdPersonController_LITE/Animator/Invector@BasicLocomotion.controller");
		if (animator == null)
		{
			Debug.LogError("AnimatorController not found at the specified path.");
			return;
		}

		Debug.Log($"AnimatorController loaded: {animator.name}");

		if (animator.layers.Length > 2)
		{
			AnimatorStateMachine rootStateMachine = animator.layers[2].stateMachine;
			if (rootStateMachine != null)
			{
				Debug.Log($"State Machine found in layer 2: {rootStateMachine.name}");
				foreach (var state in rootStateMachine.states)
				{
					Debug.Log($"Checking state: {state.state.name}");
					if (state.state.name == "Reloading")
					{
						rootStateMachine.RemoveState(state.state);
						Debug.Log("Node deleted: " + state.state.name);
						break;
					}
				}
			}
			else
			{
				Debug.LogError("State Machine is null for layer 2.");
			}
		}
		else
		{
			Debug.LogError("AnimatorController does not have enough layers.");
		}
	}

}
