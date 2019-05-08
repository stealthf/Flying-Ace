using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AMRadar : MonoBehaviour {  //anti-missile radar
    public AudioClip radarWarningClip;

    private AudioSource radarWarning;
    private List<Collider> missiles;
    private bool endangered;

    // Use this for initialization
    void Start () {
        radarWarning = gameObject.AddComponent<AudioSource>();
        radarWarning.clip = radarWarningClip;
        radarWarning.playOnAwake = false;
        radarWarning.loop = true;
        endangered = false;
        missiles = new List<Collider>();    //A queue for incoming missiles
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Missile"))
        {
            missiles.Add(other);    //add to queue
            radarWarning.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Missile"))
        {
            missiles.Remove(other); //dequeue
        }
    }

    // Update is called once per frame
    void Update () {
        endangered = false;
        if (missiles.Count > 0)
        {
            foreach (Collider missile in missiles)
            {
                if (missile != null)
                {
                    endangered = true;
                    break;
                }
            }
            if (!endangered) radarWarning.Stop();
        }
    }
}
