using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirportGround : MonoBehaviour {

    public float occupyTime = 10.0f;
    public bool playerOnGround;
    public bool airportOccupied;
    public bool supply;
    public float fuelToEdit;
    public float missileToEdit;
    public float flareToEdit;

    public Material player;
    public Material enemy;
    public GameObject icon;

    private PlaneController plane;
    private GUIStyle guiStyle = new GUIStyle();
    private GameController gameController;

    void Awake()
    {
        plane = GameObject.Find("F22-Raptor_A").GetComponent<PlaneController>();
        gameController = GameObject.Find("gameController").GetComponent<GameController>();
        if (transform.parent.tag.Equals("Airport")) icon.GetComponent<Renderer>().material = player;
        else icon.GetComponent<Renderer>().material = enemy;
    }

    void Update()
    {
        if(playerOnGround && !airportOccupied)
        {
            occupyTime -= Time.deltaTime;
            if (occupyTime <= 0)
            {
                airportOccupied = true;
                playerOnGround = false;
                transform.parent.tag = "Airport";
                supply = true;
                fuelToEdit = plane.fuel;
                flareToEdit = plane.getNumOfFlare();
                missileToEdit = plane.getNumOfMissile();
                icon.GetComponent<Renderer>().material = player;
            }
        }
        if (!playerOnGround && !airportOccupied)
            occupyTime = 10.0f;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(plane.gear)
            {
                playerOnGround = true;
            }
            if(transform.parent.tag == "Airport")
            {
                supply = true;
                fuelToEdit = plane.fuel;
                flareToEdit = plane.getNumOfFlare();
                missileToEdit = plane.getNumOfMissile();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerOnGround = false;
            if (transform.parent.tag == "Airport")
                supply = false;
        }
    }

    void OnGUI()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.grey);
        texture.Apply();
        guiStyle.fontSize = 60;
        guiStyle.normal.textColor = Color.white;
        guiStyle.normal.background = texture;
        if (supply && !(gameController.win))
        {
            GUI.Box(new Rect(400, 200, 1100, 500), "", guiStyle);
            GUI.Label(new Rect(500, 300, 300, 60), "Fuel", guiStyle);
            GUI.Label(new Rect(500, 400, 300, 60), "Missile", guiStyle);
            GUI.Label (new Rect(500, 500, 300, 60), "Flare", guiStyle);
            fuelToEdit = GUI.HorizontalSlider(new Rect(850, 330, 300, 60), fuelToEdit, 0.0F, 1.0F);
            missileToEdit = GUI.HorizontalSlider(new Rect(850, 430, 300, 60), missileToEdit, 0.0F, 24.0F - flareToEdit);
            flareToEdit = GUI.HorizontalSlider(new Rect(850, 530, 300, 60), flareToEdit, 0.0F, 24.0F - missileToEdit);
            GUI.Label(new Rect(1200, 300, 300, 60), (Mathf.Ceil(100 * fuelToEdit)).ToString() + "%", guiStyle);
            GUI.Label(new Rect(1200, 400, 300, 60), (Mathf.Ceil(missileToEdit)).ToString(), guiStyle);
            GUI.Label(new Rect(1200, 500, 300, 60), (Mathf.Ceil(flareToEdit)).ToString(), guiStyle);
            if (GUI.Button(new Rect(500, 600, 100, 60), "OK", guiStyle))
            {
                plane.fuel = fuelToEdit;
                plane.setNumOfFlare((int)Mathf.Ceil(flareToEdit));
                plane.setNumOfMissile((int)Mathf.Ceil(missileToEdit));
                plane.hitByMissile = false;
                if (plane.disabledEngine != null) plane.disabledEngine.enableEmission = true;
                supply = false;
            }
        }
    }
}
