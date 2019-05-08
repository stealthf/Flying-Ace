using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlaneCollision : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            transform.parent.GetComponent<AIPlaneController>().planeExplode();
    }
}
