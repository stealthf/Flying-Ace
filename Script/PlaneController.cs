using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{ //modified based on the "Vehicle" package
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

    //Constants for physics
    public const float MAX_THRUST = 500f;
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
    private const float FUEL_MAX_WEIGHT = 4f;
    private const float PLANE_WEIGHT = 4f;
    private const float MISSILE_WEIGHT = 0.1f;
    private const float COEEFICIENT_FUEL = 0.00002f;
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
    public float fuel { get; set; }
    public bool hitByMissile { get; set; }
    public ParticleSystem disabledEngine;

    private int numOfFlare;
    public int getNumOfFlare() { return numOfFlare; }
    public void setNumOfFlare(int _num) { numOfFlare = _num; flareButton.GetComponentInChildren<Text>().text = "Flare\n(" + numOfFlare + ")"; }
    private int numOfMissile;
    public int getNumOfMissile() { return numOfMissile ; }
    public void setNumOfMissile(int _num) { numOfMissile = _num; missileButton.GetComponentInChildren<Text>().text = "Launch\nMissile\n(" + numOfMissile + ")"; }


    public Text PFD;    //Primary Flight Display
    public Button flareButton;
    public Button missileButton;
    public Transform flareSpawnPoint;
    public ParticleSystem explosion;
    public GameObject flare;
    [HideInInspector]
    public float flareTime;

    private Rigidbody planeRigidbody;
    private float aeroFactor;
    private float baseDrag;
    private float baseAngularDrag;
    private float bank;
    private float gearState;
    private float gearTime;
    private float speedBrakeState;
    private float speedBrakeTime;
    private int flareCounter;

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
        speedBrakeState = 0;
        speedBrakeTime = 0;
        flareTime = 0;
        flareCounter= 0;
        fuel = 0f;
        numOfFlare = 0;
        numOfMissile = 0;
        hitByMissile = false;
        planeRigidbody.mass = PLANE_WEIGHT + fuel * FUEL_MAX_WEIGHT + (numOfFlare + numOfFlare) * MISSILE_WEIGHT;
    }

    public void move(float _roll, float _pitch, float _throttle)
    {
        rollInput = Mathf.Clamp(_roll, -1, 1);
        pitchInput = Mathf.Clamp(_pitch, -1, 1);
        throttle = _throttle;
        fuel -= throttle * COEEFICIENT_FUEL;
        if (fuel <= 0)
        {
            fuel = 0;
            throttle = 0;
        }
        planeRigidbody.mass = PLANE_WEIGHT + fuel * FUEL_MAX_WEIGHT + (numOfFlare + numOfFlare) * MISSILE_WEIGHT;
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
        thrust = MAX_THRUST * throttle * (hitByMissile ? 0.5f : 1);
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

    public void moveSpeedbrake()
    {
        speedBrakeState = airBrake ? -0.5f : 0.5f;
    }

    public void planeExplode()
    {
        Instantiate(explosion, transform.position, transform.rotation);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<PlaneAudio>().isFreezed = true;
        Invoke("restart", 2.5f);
    }

    public void missileExplode()
    {
        if (!hitByMissile)
        {
            Instantiate(explosion, transform.position, transform.rotation);
            hitByMissile = true;
            disabledEngine = transform.Find("Particles/" + (Random.Range(0, 2) == 1 ? "EngineLeft" : "EngineRight")).GetComponent<ParticleSystem>();
            disabledEngine.enableEmission = false;
        }
        else planeExplode();
    }

    public void createFlare()
    {
        if (numOfFlare > 0)
        {
            numOfFlare--;
            flareTime = 1f;
            flareCounter = (flareCounter > 0) ? flareCounter - 7 : 0;
        }
        flareButton.GetComponentInChildren<Text>().text = "Flare\n(" + numOfFlare + ")";
    }

    void restart()
    {
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update () {
        PFD.text = "Speed: " + Mathf.Floor(1.94f * speed) + " knots, Vertical Speed: " + (vSpeed > -1? "":"-") + Mathf.Floor(Mathf.Abs(vSpeed)) + (Mathf.Abs(vSpeed) >= 1? "00 " : " ") +"ft/min, Altitude: " + Mathf.Floor(altitude) + " ft, Fuel: " + Mathf.Ceil(100 * fuel) + "%";
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
        //extend or retract speed brake
        if (speedBrakeState == 0.5)
        {
            if (speedBrakeTime < 1)
            {
                transform.Find("F22_Rudder_L").Rotate(Vector3.right, -15 * Time.deltaTime);
                transform.Find("F22_Rudder_R").Rotate(Vector3.right, 15 * Time.deltaTime);
                speedBrakeTime += Time.deltaTime;
            }
            else
            {
                speedBrakeTime = 0;
                airBrake = true;
                speedBrakeState = 1;
            }
        }
        else if (speedBrakeState == -0.5)
        {
            if (speedBrakeTime < 1)
            {
                transform.Find("F22_Rudder_L").Rotate(Vector3.right, 15 * Time.deltaTime);
                transform.Find("F22_Rudder_R").Rotate(Vector3.right, -15 * Time.deltaTime);
                speedBrakeTime += Time.deltaTime;
            }
            else
            {
                speedBrakeTime = 0;
                airBrake = false;
                speedBrakeState = 0;
            }
        }
        //release flare
        if (flareTime > 0)
        {
            flareTime += Time.deltaTime;
            if (flareTime > 1.3f)
            {
                GameObject tempFlare = Instantiate(flare, flareSpawnPoint.position, flareSpawnPoint.rotation);
                tempFlare.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
                flareTime = 1f;
                flareCounter++;
            }
            if (flareCounter >= 7)
            {
                flareTime = 0;
                flareCounter = 0;
            }
        }
    }
}
