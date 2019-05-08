using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public GameObject prefab;   //the airport
    public Text winText;
    public Text tutText;
    public AudioClip winClip;
    [HideInInspector]
    public bool win;

    private AudioSource winSound;
    private Vector3[] airportPos;
    private GUIStyle guiStyle = new GUIStyle();
    private GameObject player;
    private bool audioPlayed;
    private int level;
    private GameObject[] enmeyNumber;   //enemy airports
    private bool chooseLevel;

    void Start()
    {
        player = GameObject.Find("F22-Raptor_A");
        chooseLevel = true;
        airportPos = new Vector3[7];
        airportPos[0] = new Vector3(26512.0F, 251.0F, 34118.0F);
        airportPos[1] = new Vector3(15308.0F, 251.0F, 35041.0F);
        airportPos[2] = new Vector3(19170.0F, 150.0F, 11916.0F);
        airportPos[3] = new Vector3(6151.0F, 200.0F, 23545.0F);
        airportPos[4] = new Vector3(6211.0F, 200.0F, 15185.0F);
        airportPos[5] = new Vector3(40571.0F, 200.0F, 12949.0F);
        airportPos[6] = new Vector3(27241.0F, 150.0F, 7077.0F);
        shuffelArray(airportPos);
        winSound = gameObject.AddComponent<AudioSource>();
        winSound.playOnAwake = false;
        winSound.clip = winClip;
        winSound.loop = false;
        win = false;
        audioPlayed = false;
    }

    void Update()
    {
        if (!chooseLevel)
        {
            enmeyNumber = GameObject.FindGameObjectsWithTag("EnemyAirport");
            if (enmeyNumber.Length == 0)
            {
                win = true;
                if (!audioPlayed)
                {
                    winSound.Play();
                    audioPlayed = true;
                }
                winText.text = "You Win!";
                Invoke("reloadScene", 2);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    private void reloadScene() { SceneManager.LoadScene(0); }

    public void setAirport()
    {
        tutText.text = "Capture all airports by landing on them!";
        Invoke("dismissText", 3f);
        prefab.tag = "Airport";
        GameObject playerAirport = Instantiate(prefab, airportPos[0], Quaternion.identity);
        playerAirport.transform.Find("surfaceTrigger").GetComponent<AirportGround>().airportOccupied = true;
        Transform spawnPoint = playerAirport.transform.Find("initialEnemy");
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
        player.GetComponent<Rigidbody>().isKinematic = false;
        prefab.tag = "EnemyAirport";
        for (int i=0; i<level; i++) //Instantiate enemy airports
        {
            Instantiate(prefab, airportPos[i+1], Quaternion.identity);
        }
    }

    private void dismissText()
    {
        tutText.text = "";
    }

    void OnGUI()
    {
        if (chooseLevel)
        {
            Time.timeScale = 0;
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.grey);
            texture.Apply();
            guiStyle.fontSize = 60;
            guiStyle.normal.textColor = Color.white;
            guiStyle.normal.background = texture;
            GUI.Box(new Rect(600, 50, 700, 900), "", guiStyle);
            GUI.Label(new Rect(700, 100, 200, 60), "Please choose level", guiStyle);
            if (GUI.Button(new Rect(700, 200, 300, 60), "Level 1", guiStyle))
            {
                level = 1;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 300, 300, 60), "Level 2", guiStyle))
            {
                level = 2;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 400, 300, 60), "Level 3", guiStyle))
            {
                level = 3;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 500, 300, 60), "Level 4", guiStyle))
            {
                level = 4;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 600, 300, 60), "Level 5", guiStyle))
            {
                level = 5;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 700, 300, 60), "Level 6", guiStyle))
            {
                level = 6;
                chooseLevel = false;
                Time.timeScale = 1;
                setAirport();
            }
            if (GUI.Button(new Rect(700, 800, 300, 60), "Quit game", guiStyle))
            {
                Application.Quit();
            }
        }
        else
            if (GUI.Button(new Rect(50, 50, 320, 60), "Main menu", guiStyle))
            {
                SceneManager.LoadScene(0);
            }
    }

    private void shuffelArray(Vector3[] arr)
    {
        for (int t = 0; t < arr.Length; t++)    //t=2 for debug, =0 for release
        {
            Vector3 tmp = arr[t];
            int r = Random.Range(t, arr.Length);
            arr[t] = arr[r];
            arr[r] = tmp;
        }
    }
}