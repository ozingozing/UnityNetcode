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

	public CinemachineBlenderSettings.CustomBlend[] customBlends;
	public float[] defaultChangingValues;
	private void Start()
	{
		MapViewCam.Priority = 15;
		customBlends = GetComponent<CinemachineBrain>().m_CustomBlends.m_CustomBlends;
		defaultChangingValues = new float[customBlends.Length];
		for(int i = 0; i < customBlends.Length; i ++)
		{
			defaultChangingValues[i] = customBlends[i].m_Blend.m_Time;
		}
	}

	private void OnDestroy()
	{
		ResetAllBlendsValue();
	}

	public void ResetAllBlendsValue()
	{
		for (int i = 0; i < customBlends.Length; i++)
		{
			customBlends[i].m_Blend.m_Time = defaultChangingValues[i];
		}
	}

	public void CamChange(int index, float time)
	{
		if (index < 0 || index >= customBlends.Length)
		{
			Debug.LogError("Index Out of Range!!!");
		}
		StartCoroutine(Change(index, time));
	}

	IEnumerator Change(int index, float time)
	{
		customBlends[index].m_Blend.m_Time = time;

		yield return new WaitForSeconds(time * 1.12f);

		customBlends[index].m_Blend.m_Time = defaultChangingValues[index];
	}
}
