using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlaneParticle : MonoBehaviour
{//modified based on the "Vehicle" package

    public Color baseColor;

    private AIPlaneController plane;
    private ParticleSystem particle;
    private float initStartSize;
    private float initLifeTime;
    private Color initStartColor;

	// Use this for initialization
	void Start () {
        plane = transform.parent.parent.GetComponent<AIPlaneController>();
        particle = GetComponent<ParticleSystem>();
        initLifeTime = particle.main.startLifetime.constant;
        initStartSize = particle.main.startSize.constant;
        initStartColor = particle.main.startColor.color;
	}
	
	// Update is called once per frame
	void Update () {
        ParticleSystem.MainModule mainModule = particle.main;
        mainModule.startLifetime = Mathf.Lerp(0f, initLifeTime, plane.throttle);
        mainModule.startSize = Mathf.Lerp(initStartSize * 0.3f, initStartSize, plane.throttle);
        mainModule.startColor = Color.Lerp(baseColor, initStartColor, plane.throttle);
	}
}
