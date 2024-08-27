using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
	[HideInInspector] public static TestRelay Instance {  get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	[Command]
	public async Task<string> CreateRelay()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			Debug.Log($"{joinCode}");

			RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
			/*NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
					allocation.RelayServer.IpV4,
					(ushort)allocation.RelayServer.Port,
					allocation.AllocationIdBytes,
					allocation.Key,
					allocation.ConnectionData
				);*/
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

			NetworkManager.Singleton.StartHost();

			return joinCode;
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);

			return null;
		}
	}

	[Command]
	public async void JoinRelay(string joinCode)
	{
		try
		{
			Debug.Log($"Join relay: {joinCode}");
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
			/*NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
					joinAllocation.RelayServer.IpV4,
					(ushort)joinAllocation.RelayServer.Port,
					joinAllocation.AllocationIdBytes,
					joinAllocation.Key,
					joinAllocation.ConnectionData,
					joinAllocation.HostConnectionData
				);*/

			NetworkManager.Singleton.StartClient();
		}
		catch (RelayServiceException e)
		{
			Debug.Log(e);
		}
	}
}
