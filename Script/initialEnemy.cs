using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initialEnemy : MonoBehaviour { //instantiate enemy jet in airport

    public GameObject Enemy;
    public float enemyRate;
    private AirportGround airportGround;
    private bool launchEnemy;

    void Start () {
        enemyRate = 50;
        airportGround = transform.parent.Find("surfaceTrigger").GetComponent<AirportGround>();
    }


    void Update()
    {
        if(launchEnemy && enemyRate >= 50 && !airportGround.airportOccupied)
        {
            Instantiate(Enemy, transform.position, transform.rotation);
        }
        if (launchEnemy) enemyRate -= Time.deltaTime;
        if (enemyRate <= 0)
        {
            enemyRate = 50.0f;
        }
    }
	
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.transform.parent.tag == "EnemyAirport")
        {
            launchEnemy = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && gameObject.transform.parent.tag == "EnemyAirport")
        {
            launchEnemy = false;
        }
    }
}
