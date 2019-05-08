using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* The radar script for AI */

public class AIARadar : MonoBehaviour {
    private float lockTime;
    private GameObject missileTarget;

    public GameObject missile;

	void Start () {
        lockTime = 0;
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            missileTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            missileTarget = null;
    }

    void Update () {
        if (missileTarget != null)
            lockTime += Time.deltaTime;
        else lockTime = 0;
        if (lockTime >= 10)
        {
            launchMissile();
            lockTime = 0;
        }
    }

    public void launchMissile()
    {
        Instantiate(missile, new Vector3(transform.position.x, transform.position.y, transform.position.z + 10f), transform.rotation);
    }
}
