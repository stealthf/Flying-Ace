using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetIcon : MonoBehaviour {

    public Transform target;

	// Use this for initialization
	void Start () {
        transform.position = new Vector3(0, 4000, 0);
        transform.eulerAngles = new Vector3(-90, 0, 90);
	}
	
	// Update is called once per frame
	void Update () {
        if (target == null) Destroy(gameObject); 
        else
        {
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, target.eulerAngles.y, transform.eulerAngles.z);
        }
    }
}
