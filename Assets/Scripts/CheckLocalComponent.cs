using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckLocalComponent : NetworkBehaviour
{
    [SerializeField] private Transform local3rdCam;


	// Start is called before the first frame update
	void Start()
    {
        if (IsLocalPlayer)
        {
            local3rdCam.gameObject.SetActive(true);
        }
    }
}
