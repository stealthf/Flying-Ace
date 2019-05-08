using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMFlare : MonoBehaviour {//Anti-missile flare
    private float timer;

	// Use this for initialization
	void Start () {
        timer = 0f;
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        if (timer >= 7f)
            Destroy(this.gameObject);
	}

    private void FixedUpdate()
    {
        GetComponent<Rigidbody>().AddForce(Physics.gravity * 5f, ForceMode.Acceleration);   //make it fall quicker
    }
}
