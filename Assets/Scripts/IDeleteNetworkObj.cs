using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IDeleteNetworkObjId
{
	[ClientRpc]
	public void DeleteRequestClientRpc(ulong id);

	[ServerRpc(RequireOwnership = false)]
	public void DeleteRequestServerRpc(ulong id);
}

public interface IDeleteNetworkObj
{
	[ClientRpc]
	public void DeleteRequestClientRpc(NetworkObjectReference networkObjectReference);

	[ServerRpc(RequireOwnership = false)]
	public void DeleteRequestServerRpc(ulong id);
}

