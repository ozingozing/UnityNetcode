using ChocoOzing.EventBusSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager
{
    Animator animator;

	static readonly int HipFireHash = Animator.StringToHash("HipFire");
	
	static readonly int ReloadHash = Animator.StringToHash("Reloading");
	static readonly int ManyReloadHash = Animator.StringToHash("ManyReload");
	static readonly int ShotgunReloadActionHash = Animator.StringToHash("ShotgunReloadAction");
	static readonly int ShotgunSetPosHash = Animator.StringToHash("ShotgunSetPos");
	static readonly int ShotgunPumpActionHash = Animator.StringToHash("ShotgunPumpAction");


	readonly Dictionary<int, float> animationDuration = new()
    {
		{ HipFireHash, 0.1f },
        { ReloadHash, 2.9f },
		{ ManyReloadHash, 0.2f },
		{ ShotgunReloadActionHash, 0.25f },
		{ ShotgunSetPosHash, 0.3f },
		{ ShotgunPumpActionHash, 0.4f },
    };

    const float corossFadeDuration = 0.1f;

	public AnimationManager(Animator animator)
	{
		this.animator = animator;
	}

	public float HipFire() => PlayAnimation(HipFireHash, corossFadeDuration, 2);
	public float HipFire(float durtaion) => PlayAnimation(HipFireHash, durtaion, 2);

	public float Reload() => PlayAnimation(ReloadHash, corossFadeDuration, 2);
	public float ManyReload() => PlayAnimation(ManyReloadHash, corossFadeDuration, 2);
	public float ShotgunReloadAction() => PlayAnimation(ShotgunReloadActionHash, 0.2f, 2);
	public float ShotgunSetPos() => PlayAnimation(ShotgunSetPosHash, 0.25f, 2);
	public float ShotgunPumpAction() => PlayAnimation(ShotgunPumpActionHash, corossFadeDuration, 2);

	float PlayAnimation(int animationHash, float duration, int layer)
	{
		animator.CrossFade(animationHash, duration, layer);
		return animationDuration[animationHash];
	}

	float PlayAnimation(int animationHash)
    {
        animator.CrossFade(animationHash, corossFadeDuration);
        return animationDuration[animationHash];
    }
}
