using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Invoke("destroySelf", 3);
	}

    void destroySelf() { Destroy(this.gameObject); }
}
