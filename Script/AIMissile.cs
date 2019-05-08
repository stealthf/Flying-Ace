using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMissile : MonoBehaviour {

    public ParticleSystem explosion;

    private GameObject target;
    private bool dodged;
    private float speed = 600f;
    private Vector3 lastPos;
    private float timer = 0f;

    void Start () {
        target = GameObject.Find("F22-Raptor_A");   //the player jet
        dodged = false;
    }

    void FixedUpdate () {
        timer += Time.deltaTime;
        if (timer > 10f) Destroy(gameObject);
        if (Vector3.Distance(transform.position, target.transform.position) > 200 || (Vector3.Distance(transform.position, target.transform.position) < 50 && !dodged))
        //if distance > 200 or < 50 and not dodged, follow the player
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
        }
        else
        {
            if (!dodged)
            {
                if (target.GetComponent<PlaneController>().flareTime != 0)  //if flare still exists
                {
                    dodged = true;
                    lastPos = target.transform.position;
                }
                else
                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
            }
            else//if dodged move to the last known position
            {
                transform.position = Vector3.MoveTowards(transform.position, lastPos, Time.deltaTime * speed);
                if (Vector3.Distance(transform.position, target.transform.position) > 200)
                    Destroy(gameObject);
            }
        }
        if (Vector3.Distance(transform.position, target.transform.position) == 0)   //if hit
        {
            Destroy(gameObject);
            target.GetComponent<PlaneController>().missileExplode();
        }
    }
}
