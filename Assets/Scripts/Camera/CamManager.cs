using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    public static CamManager Instance;

	public CinemachineVirtualCamera AdsCam;
	public CinemachineVirtualCamera ThirdPersonCam;
	public CinemachineVirtualCamera MapViewCam;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		MapViewCam.Priority = 15;
	}

}
