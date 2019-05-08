using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {
    private const float ROLL_COEFFICIENT = 0.2f;
    private const float PITCH_COEFFICIENT = 0.5f;
    private const float MAX_VANGLE = 60;
    private const float MAX_ROLL = 60;
    private const float SPEED_COEFFICIENT = 0.01f;
    private const float STOP_CLIMB_HEIGHT = 500;

    private Transform target;
    private AIPlaneController planeController;
    private bool stopClimb;

    private void Awake()
    { 
        planeController = GetComponent<AIPlaneController>();
        target = GameObject.Find("F22-Raptor_A").transform;
    }

    private void FixedUpdate()  //modified based on the AI script from "Vehicle" package
    {
        Vector3 targetPos = target.position;
        Vector3 localTarget = transform.InverseTransformPoint(targetPos);
        float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
        float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);
        targetAnglePitch = Mathf.Clamp(targetAnglePitch, -MAX_VANGLE * Mathf.Deg2Rad,
                                        MAX_VANGLE * Mathf.Deg2Rad);
        float changePitch = targetAnglePitch - planeController.pitch;
        float throttleInput = stopClimb ? 0.8f : 1f;
        float pitchInput = changePitch * PITCH_COEFFICIENT;
        float desiredRoll = Mathf.Clamp(targetAngleYaw, -MAX_ROLL * Mathf.Deg2Rad, MAX_ROLL * Mathf.Deg2Rad);
        float rollInput = 0;
        if (!stopClimb)
        {
            if (planeController.radioAltitude > STOP_CLIMB_HEIGHT)
            {
                stopClimb = true;
            }
        }
        else
        {
            rollInput = -(planeController.roll - desiredRoll) * ROLL_COEFFICIENT;
        }
        float currentSpeedEffect = 1 + (planeController.speed * SPEED_COEFFICIENT);
        rollInput *= currentSpeedEffect;
        pitchInput *= currentSpeedEffect;
        if (!stopClimb) pitchInput = -1;
        planeController.move(rollInput, pitchInput, throttleInput);
        planeController.moveComponent(rollInput, pitchInput);
    }
}
