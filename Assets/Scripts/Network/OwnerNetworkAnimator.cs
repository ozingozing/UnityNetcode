using UnityEngine;
using Unity.Netcode.Components;

[DisallowMultipleComponent]
public class OwnerNetworkAnimator : NetworkAnimator
{
	protected override bool OnIsServerAuthoritative()
	{
		return false;
	}
}

