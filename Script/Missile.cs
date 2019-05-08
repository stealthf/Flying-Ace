using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {
    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public Vector3 direction;

    public ParticleSystem explosion;

    private float speed = 600f;

    private float timer = 0f;

	void Awake () {
        target = null;
	}

    // Update is called once per frame
    void FixedUpdate () {
        timer += Time.deltaTime;
        if (timer > 10f) Destroy(gameObject);
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
        }
        else
        {
            GetComponent<Rigidbody>().position += direction.normalized * speed; //directy shoot out
        }       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(other.CompareTag("Radar") || other.CompareTag("Airport") || other.CompareTag("Terrain") || other.CompareTag("EnemyAirport") || other.CompareTag("Untagged")))
        {
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(this.gameObject);
            Destroy(other.gameObject);
        }
    }
}
