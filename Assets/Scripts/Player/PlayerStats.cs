using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> kills = new NetworkVariable<int>();
    public NetworkVariable<int> deaths = new NetworkVariable<int>();

    public void AddKill()
    {
        if (IsServer) kills.Value++;
    }

    public void AddDeath()
    {
        if (IsServer) deaths.Value++;
    }
}
