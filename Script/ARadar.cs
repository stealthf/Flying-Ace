using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARadar : MonoBehaviour {   //Attacking radar
    private List<GameObject> targets;   //A target queue
    private Camera mainCamera;
    private GameObject lastTarget;
    private float lockTime;
    private GameObject missileTarget;
    private PlaneController planeController;
    private AudioSource lockSound;

    public GameObject missile;
    public Texture redBox;
    public Texture greenBox;
    public Button missileButton;
    public AudioClip lockClip;
    public Transform missileSpawnPoint;

    private float halfLockerSize = 32f;

	// Use this for initialization
	void Start () {
        targets = new List<GameObject>();
        mainCamera = GameObject.Find("F22-Raptor_A/MainCamera").GetComponent<Camera>();
        lockTime = 0;
        lastTarget = null;
        planeController = transform.parent.GetComponent<PlaneController>();
        lockSound = gameObject.AddComponent<AudioSource>();
        lockSound.playOnAwake = true;
        lockSound.clip = lockClip;
        lockSound.loop = true;
        lockSound.volume = 0;
        lockSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Cannon") && other.transform.parent.CompareTag("EnemyAirport")) || other.CompareTag("Enemy"))
        {
            targets.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Cannon") && other.transform.parent.CompareTag("EnemyAirport")) || other.CompareTag("Enemy"))
        {
            targets.Remove(other.gameObject);
            lastTarget = null;
            missileTarget = null;
        }
    }

    // Update is called once per frame
    void Update () {
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    lockSound.volume = 1;
                    if (lastTarget == targets[i])
                        lockTime += Time.deltaTime;
                    else
                    {
                        lockTime = 0;
                        lastTarget = targets[i];
                    }
                    break;
                }
                else lockSound.volume = 0;
            }
        }
        else lockSound.volume = 0;
    }

    private void OnGUI()
    {
        if (targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null)
                {
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(targets[i].transform.position);
                    if (lockTime < 3)//locking
                    {
                        lockSound.pitch = 1;
                        GUI.Box(new Rect(screenPos.x - halfLockerSize, mainCamera.pixelHeight - screenPos.y - halfLockerSize, 2 * halfLockerSize, 2 * halfLockerSize), redBox);
                    }                        
                    else//locked
                    {
                        lockSound.pitch = 2;
                        GUI.Box(new Rect(screenPos.x - halfLockerSize, mainCamera.pixelHeight - screenPos.y - halfLockerSize, 2 * halfLockerSize, 2 * halfLockerSize), greenBox);
                        missileTarget = lastTarget;
                    }                        
                    break;
                }
            }
        }
    }

    public void launchMissile()
    {
        if (planeController.getNumOfMissile() > 0)
        {
            planeController.setNumOfMissile(planeController.getNumOfMissile() - 1);
            GameObject tempMissile = Instantiate(missile, missileSpawnPoint.position, missileSpawnPoint.rotation);
            tempMissile.GetComponent<Missile>().target = missileTarget;
            tempMissile.GetComponent<Missile>().direction = Vector3.forward;
            nextTarget();
        }
        missileButton.GetComponentInChildren<Text>().text = "Launch\nMissile\n(" + planeController.getNumOfMissile() + ")";
    }

    public void nextTarget()//move the first target in the queue to last by dequene and enqueue again
    {
        if (targets.Count > 0)
        {
            GameObject tempTarget = targets[0];
            targets.RemoveAt(0);
            targets.Add(tempTarget);
        }
    }
}
