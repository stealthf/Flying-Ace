using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAudio : MonoBehaviour
{ //modified based on the "Vehicle" package
    [System.Serializable]
    public class AdvancedSetttings // A class for storing the advanced options.
    {
        public float engineMinDistance = 50f;
        public float engineMaxDistance = 1000f;
        public float engineDopplerLevel = 1f;
        [Range(0f, 1f)]
        public float engineMasterVolume = 0.5f;
        public float windMinDistance = 10f;
        public float windMaxDistance = 100f;
        public float windDopplerLevel = 1f;
        [Range(0f, 1f)]
        public float windMasterVolume = 0.5f;
    }

    [SerializeField]
    private AudioClip engineClip;
    [SerializeField]
    private float minEnginePitch = 0.4f;
    [SerializeField]
    private float maxEnginePitch = 2f;
    [SerializeField]
    private float speedCoefficient = 0.002f;
    [SerializeField]
    private AudioClip windClip;
    [SerializeField]
    private float baseWindPitch = 0.2f;
    [SerializeField]
    private float windCoefficient = 0.004f;
    [SerializeField]
    private float maxWindVolume = 100;
    [SerializeField]
    private AdvancedSetttings advanced = new AdvancedSetttings();

    private AudioSource engineSound;
    private AudioSource windSound;
    private PlaneController planeController;
    private Rigidbody rigidBody;
    [HideInInspector]
    public bool isFreezed;

    private void Awake()
    {
        // Set up the reference to the aeroplane controller.
        planeController = GetComponent<PlaneController>();
        rigidBody = GetComponent<Rigidbody>();
        engineSound = gameObject.AddComponent<AudioSource>();
        engineSound.playOnAwake = false;
        windSound = gameObject.AddComponent<AudioSource>();
        windSound.playOnAwake = false;
        engineSound.clip = engineClip;
        windSound.clip = windClip;
        engineSound.minDistance = advanced.engineMinDistance;
        engineSound.maxDistance = advanced.engineMaxDistance;
        engineSound.loop = true;
        engineSound.dopplerLevel = advanced.engineDopplerLevel;
        windSound.minDistance = advanced.windMinDistance;
        windSound.maxDistance = advanced.windMaxDistance;
        windSound.loop = true;
        windSound.dopplerLevel = advanced.windDopplerLevel;
        Update();
        engineSound.Play();
        windSound.Play();
    }


    private void Update()
    {
        if (!isFreezed)
        {
            var enginePowerProportion = Mathf.InverseLerp(0, PlaneController.MAX_THRUST, planeController.thrust);
            engineSound.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, enginePowerProportion);
            engineSound.pitch += planeController.speed * speedCoefficient;
            engineSound.volume = Mathf.InverseLerp(0, PlaneController.MAX_THRUST * advanced.engineMasterVolume, planeController.thrust);
            float planeSpeed = rigidBody.velocity.magnitude;
            windSound.pitch = baseWindPitch + planeSpeed * windCoefficient;
            windSound.volume = Mathf.InverseLerp(0, maxWindVolume, planeSpeed) * advanced.windMasterVolume;
        }
        else
        {
            engineSound.volume = windSound.volume = 0;
        }
    }
}
