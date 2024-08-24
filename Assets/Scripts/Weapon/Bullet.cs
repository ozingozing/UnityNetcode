using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float timeToDestroy;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > timeToDestroy ) Destroy(gameObject);
    }

	private void OnCollisionEnter(Collision collision)
	{
        if(collision.gameObject.layer == LayerMask.NameToLayer("Default"))
		    Destroy(gameObject);
	}
}
