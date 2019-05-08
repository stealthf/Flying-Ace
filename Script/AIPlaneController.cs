using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class AIPlaneController : MonoBehaviour
{    //modified based on the "Vehicle" package
    [System.Serializable]
    public class Surface
    {
        public enum Type // Flaps differ in position and rotation and are represented by different types.
        {
            AileronL,
            AileronR,
            ElevatorL,
            ElevatorR,
            Engine,
            Flame
        }

        public Transform transform;
        public Type type;
        public int moveSpeed;

        [HideInInspector]
        public Quaternion baseRotation;
    }

    public GameObject jetIcon;
    public Material iconMaterial;

    //Constants for physics
    public const float MAX_THRUST = 400f;
    private const float MAX_SPEED = 300f;
    private const float LIFT = 0.005f;
    private const float COEFFICIENT_DRAG = 0.0012f;
    private const float COEFFICIENT_ROLL = 2f;
    private const float COEFFICIENT_PITCH = 2f;
    private const float COEFFICIENT_BANK = 1f;
    private const float COEFFICIENT_AIR = 0.02f;
    private const float PITCH_TURN = 0.5f;
    private const float LEVEL_ROLL = 0.2f;
    private const float LEVEL_PITCH = 0.2f;
    private const float AIR_BRAKE = 3f;
    private const float GEAR_DRAG = 0.2f;
    private const float DELTA_THURST = 0.3f;
    private const float PLANE_WEIGHT = 8f;

    private const int AILERON_ANGLE_MAX = 30;
    private const int ELEVATOR_ANGLE_MAX = 30;
    private const int ENGINE_ANGLE_MAX = 30;
    private const int FLAP_ANGLE_MAX = 30;
    private const int RUDDER_ANGLE_MAX = 10;

    public float radioAltitude { get; private set; }
    public float altitude { get; private set; }
    public float thrust { get; private set; }
    public bool airBrake { get; private set; }
    public bool gear { get; private set; }
    public float speed { get; private set; }
    public float vSpeed { get; private set; }
    public float throttle { get; private set; }
    public float roll { get; private set; }
    public float pitch { get; private set; }
    public float throttleInput { get; private set; }
    public float rollInput { get; private set; }
    public float pitchInput { get; private set; }

    public ParticleSystem explosion;

    private Rigidbody planeRigidbody;
    private float aeroFactor;
    private float baseDrag;
    private float baseAngularDrag;
    private float bank;
    private float gearState;
    private float gearTime;

    private GameObject icon;

    [SerializeField] private Surface[] surfaces;

    // Use this for initialization
    void Start () {
        planeRigidbody = GetComponent<Rigidbody>();
        planeRigidbody.centerOfMass = new Vector3(0, 0.75f, -1f);
        baseDrag = planeRigidbody.drag;
        baseAngularDrag = planeRigidbody.angularDrag;
        for (int i = 0; i < transform.childCount; i++)
            foreach (WheelCollider wheel in transform.GetChild(i).GetComponentsInChildren<WheelCollider>())
                wheel.motorTorque = 0.18f;
        foreach (Surface surface in surfaces)
        {
            surface.baseRotation = surface.transform.localRotation;
        }
        gear = true;
        gearState = 1;
        gearTime = 0;
        airBrake = false;
        planeRigidbody.mass = PLANE_WEIGHT;
        icon = Instantiate(jetIcon);
        icon.GetComponent<JetIcon>().target = transform;
        icon.GetComponent<Renderer>().material = iconMaterial;
    }

    public void move(float _roll, float _pitch, float _throttle)
    {
        rollInput = Mathf.Clamp(_roll, -1, 1);
        pitchInput = Mathf.Clamp(_pitch, -1, 1);
        throttle = _throttle;
        //Auto-change the roll and pitch
        Vector3 flatForward = transform.forward;
        flatForward.y = 0;
        if (flatForward.magnitude != 0)
        {
            flatForward.Normalize();
            pitch = Mathf.Atan2(transform.InverseTransformDirection(flatForward).y, transform.InverseTransformDirection(flatForward).z);
            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward);
            roll = Mathf.Atan2(transform.InverseTransformDirection(flatRight).y, transform.InverseTransformDirection(flatRight).x);
        }
        //bank according to the input
        bank = Mathf.Sin(roll);
        if (rollInput == 0)
            rollInput = -roll * LEVEL_ROLL;
        if (pitchInput == 0)
            pitchInput = -pitch * LEVEL_PITCH - Mathf.Abs(Mathf.Pow(bank, 2) * PITCH_TURN);
        //determine speed
        speed = Mathf.Max(0, transform.InverseTransformDirection(planeRigidbody.velocity).z);
        vSpeed = planeRigidbody.velocity.y * 0.5f;
        //set thrust
        thrust = MAX_THRUST * throttle;
        //set drag
        float deltaDrag = planeRigidbody.velocity.magnitude * COEFFICIENT_DRAG;
        planeRigidbody.drag = (baseDrag + deltaDrag + (gear? GEAR_DRAG : 0)) * (airBrake ? AIR_BRAKE : 1);
        planeRigidbody.angularDrag = baseAngularDrag * speed;
        //make the movement more natural
        if (planeRigidbody.velocity.magnitude > 0)
        {
            aeroFactor = Mathf.Pow(Vector3.Dot(transform.forward, planeRigidbody.velocity.normalized), 2);
            planeRigidbody.velocity = Vector3.Lerp(planeRigidbody.velocity, transform.forward * speed, aeroFactor * speed * COEFFICIENT_AIR * Time.deltaTime);
            planeRigidbody.rotation = Quaternion.Slerp(planeRigidbody.rotation, Quaternion.LookRotation(planeRigidbody.velocity, transform.up), COEFFICIENT_AIR * Time.deltaTime);
        }
        //apply all forces
        Vector3 force = Vector3.zero + thrust * transform.forward;
        force += Vector3.Cross(planeRigidbody.velocity, transform.right).normalized * Mathf.Pow(speed, 2) * LIFT * Mathf.InverseLerp(MAX_SPEED, 0, speed) * aeroFactor;
        planeRigidbody.AddForce(force);
        //apply torque
        Vector3 torque = Vector3.zero + pitchInput * COEFFICIENT_PITCH * transform.right - rollInput * COEFFICIENT_ROLL * transform.forward + bank * COEFFICIENT_BANK * transform.up;
        planeRigidbody.AddTorque(torque * speed * aeroFactor);
        //get radio altitude by raycast
        Ray altimeter = new Ray(transform.position - Vector3.up * 10, -Vector3.up);
        RaycastHit hit;
        Physics.Raycast(altimeter, out hit);
        radioAltitude = hit.distance + 10;
        if (radioAltitude > 50f) moveGear();
        altitude = transform.position.y;
    }

    public void moveComponent(float _roll, float _pitch)
    {
        rollInput = Mathf.Clamp(_roll, -1, 1);
        pitchInput = Mathf.Clamp(_pitch, -1, 1);
        foreach (Surface surface in surfaces)
        {
            switch (surface.type)
            {
                case Surface.Type.AileronL:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(-AILERON_ANGLE_MAX * (rollInput > 0 ? rollInput : 0), 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
                case Surface.Type.AileronR:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(AILERON_ANGLE_MAX * (rollInput < 0 ? rollInput : 0), 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
                case Surface.Type.ElevatorL:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(-ELEVATOR_ANGLE_MAX * Mathf.Clamp(pitchInput + rollInput, -1, 1), 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
                case Surface.Type.ElevatorR:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(-ELEVATOR_ANGLE_MAX * Mathf.Clamp(pitchInput - rollInput, -1, 1), 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
                case Surface.Type.Engine:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(-ELEVATOR_ANGLE_MAX * pitchInput, 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
                case Surface.Type.Flame:
                    {
                        surface.transform.localRotation = Quaternion.Slerp(surface.transform.localRotation, surface.baseRotation * Quaternion.Euler(ELEVATOR_ANGLE_MAX * pitchInput, 0f, 0f), surface.moveSpeed * Time.deltaTime);
                        break;
                    }
            }
        }
    }

    public void moveGear()
    {
        if (gearState != 0.5f && gearState != -0.5f)
        {
            gearState = gear ? -0.5f : 0.5f;
            gear = !gear;
        }
    }

    public void planeExplode()
    {
        Instantiate(explosion, transform.position, transform.rotation);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<AIPlaneAudio>().isFreezed = true;
        Invoke("destroy", 2.5f);
    }

    void destroy()
    {
        Destroy(this.gameObject);
        Destroy(icon);
    }

    // Update is called once per frame
    void Update () {
        //raise or lower gear
        if (gearState == 0.5)
        {
            if (gearTime < 5)
            {
                transform.Find("F22_SideWheel_L").Rotate(Vector3.right, -18 * Time.deltaTime);
                transform.Find("F22_SideWheel_L/Wheel_Left").Rotate(Vector3.right, -18 * Time.deltaTime);
                transform.Find("F22_SideWheel_R").Rotate(Vector3.right, 18 * Time.deltaTime);
                transform.Find("F22_SideWheel_R/Wheel_Right").Rotate(Vector3.right, 18 * Time.deltaTime);
                transform.Find("F22_FrontWheel").Rotate(Vector3.forward, -18 * Time.deltaTime);
                transform.Find("F22_FrontWheel/Wheel_Front").Rotate(Vector3.forward, 18 * Time.deltaTime);
                if (gearTime >= 4)
                {
                    transform.Find("F22_SideWheeldoor_L").Rotate(Vector3.right, 90 * Time.deltaTime);
                    transform.Find("F22_SideWheeldoor_R").Rotate(Vector3.right, -90 * Time.deltaTime);
                }
                gearTime += Time.deltaTime;
            }
            else
            {
                gearTime = 0;
                gearState = 1;
            }
        }
        else if (gearState == -0.5)
        {
            if (gearTime < 5)
            {
                transform.Find("F22_SideWheel_L").Rotate(Vector3.right, 18 * Time.deltaTime);
                transform.Find("F22_SideWheel_L/Wheel_Left").Rotate(Vector3.right, 18 * Time.deltaTime);
                transform.Find("F22_SideWheel_R").Rotate(Vector3.right, -18 * Time.deltaTime);
                transform.Find("F22_SideWheel_R/Wheel_Right").Rotate(Vector3.right, -18 * Time.deltaTime);
                transform.Find("F22_FrontWheel").Rotate(Vector3.forward, 18 * Time.deltaTime);
                transform.Find("F22_FrontWheel/Wheel_Front").Rotate(Vector3.forward, -18 * Time.deltaTime);
                if (gearTime >= 4)
                {
                    transform.Find("F22_SideWheeldoor_L").Rotate(Vector3.right, -90 * Time.deltaTime);
                    transform.Find("F22_SideWheeldoor_R").Rotate(Vector3.right, 90 * Time.deltaTime);
                }
                gearTime += Time.deltaTime;
            }
            else
            {
                gearTime = 0;
                gearState = 0;
            }
        }
    }
}
