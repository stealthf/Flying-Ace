using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonAttack : MonoBehaviour {
    public float missileSpeed = 300.0f;
    public bool attack;

    private GameObject player;
    private PlaneController plane;
    private bool dodged;
    private Vector3 lastPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        plane = GameObject.Find("F22-Raptor_A").GetComponent<PlaneController>();
        dodged = false;
    }

    void FixedUpdate()
    {
        if (attack)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > 200 || (Vector3.Distance(transform.position, player.transform.position) < 50 && !dodged))
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * missileSpeed);
            }
            else
            {
                if (!dodged)
                {
                    if (plane.flareTime != 0)
                    {
                        dodged = true;
                        lastPos = player.transform.position;
                    }
                    else
                        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * missileSpeed);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, lastPos, Time.deltaTime * missileSpeed);
                    if (Vector3.Distance(transform.position, player.transform.position) > 200)
                        Destroy(gameObject);
                }
            }
        }
        if (Vector3.Distance(transform.position, player.transform.position) == 0)
        {
            Destroy(gameObject);
            plane.missileExplode();
        }
    }

    public void Attack()    //called outside
    {
        attack = true;
    }
}
