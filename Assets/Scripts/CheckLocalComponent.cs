using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CheckLocalComponent : NetworkBehaviour
{
    [SerializeField] private Transform local3rdCam;
    vThirdPersonCamera vThirdPersonCamera;
	// Start is called before the first frame update
	void Awake()
    {
        if (IsLocalPlayer)
        {
            vThirdPersonCamera = local3rdCam.GetComponent<vThirdPersonCamera>();
            vThirdPersonCamera.target = this.transform;
			local3rdCam.gameObject.SetActive(true);
		}
    }
}
