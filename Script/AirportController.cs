using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirportController : MonoBehaviour {

    public bool playerInRange;

    private LunchMissile launchMissile;

    void Update()
    {
        if(playerInRange && launchMissile != null)
            launchMissile.LaunchMissile();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.tag == "EnemyAirport")
        {
            playerInRange = true;
            if (transform.Find("connon"))   //if connon hasn't been destroyed by the player
            {
                transform.Find("connon/LaunchMissile").tag = "EnemyLaunchMissile";
                launchMissile = GameObject.FindGameObjectWithTag("EnemyLaunchMissile").GetComponent<LunchMissile>();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
            playerInRange = false;
    }
}
