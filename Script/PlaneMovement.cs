using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaneMovement : MonoBehaviour
{ //modified based on the "Vehicle" package
    public Slider throttleInput;

    private const float ROLL_MAX = 80f;
    private const float PITCH_MAX = 80f;

    private PlaneController planeController;
    private float throttle;
    private bool airBrake;
    private float yaw;

	// Use this for initialization
	void Awake () {
        planeController = GetComponent<PlaneController>();
	}

    private void FixedUpdate()
    {
        if (Mathf.Abs(throttle - throttleInput.value) < 0.03f)
            throttle = Mathf.Log((throttleInput.value + 1), 2);
        else
            throttle = Mathf.Lerp(throttle, Mathf.Log((throttleInput.value + 1), 2), 0.01f);
        //for PC
        /*planeController.move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), throttle);
        planeController.moveComponent(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));*/
        //for Android
        planeController.move(Input.acceleration.x, Input.acceleration.y, throttle);
        planeController.moveComponent(Input.acceleration.x, Input.acceleration.y);
    }
}