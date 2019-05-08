using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LunchMissile : MonoBehaviour {

    public GameObject homingMissile;
    private CannonAttack attack;
    private float attackRate = 10.0f;
    private bool attacking;

    void Awake()
    {
        attack = homingMissile.GetComponent<CannonAttack>();
    }

    void Update()
    {
        if(!attacking)
        {
            attackRate -= Time.deltaTime;
        }
        if (attackRate <= 0)
        {
            attacking = true;
            attackRate = 10.0f;
        }
    }

    public void LaunchMissile()
    {
        if(attacking)
        {
            Instantiate(homingMissile, transform.position, transform.rotation);
            attack.Attack();
            attacking = false;
        }
    }
}
