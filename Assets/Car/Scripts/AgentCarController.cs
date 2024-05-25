using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class CarAgentController : Agent
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialParkingPlacePosition;
    private Quaternion initialParkingPlaceRotation;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isBreaking;
    private bool isParked;
    private int parkingZoneLayer;

    [Header("Car Settings")]
    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteerAngle;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [Header("Wheel Mesh Transforms")]
    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [Header("Environment")]
    [SerializeField] private GameObject parkingPlace;
    [SerializeField] private Transform border;

    [Header("Agent Settings")]
    [SerializeField] private short numBeaconRequired = 6;
    [SerializeField] private float parkingThreshold = 0.25f;
    [SerializeField] private float timeToPark = 80f;

    public override void Initialize()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialParkingPlacePosition = parkingPlace.transform.position;
        initialParkingPlaceRotation = parkingPlace.transform.rotation;
        parkingZoneLayer = LayerMask.NameToLayer("ParkingZone");
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        parkingPlace.transform.position = initialParkingPlacePosition;
        parkingPlace.transform.rotation = initialParkingPlaceRotation;
        isParked = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(transform.localEulerAngles.y);

        sensor.AddObservation(frontLeftWheelCollider.rpm);

        sensor.AddObservation(parkingPlace.transform.localPosition.x);
        sensor.AddObservation(parkingPlace.transform.localPosition.z);
        sensor.AddObservation(parkingPlace.transform.localEulerAngles.y);

        sensor.AddObservation(Vector3.Distance(transform.position, parkingPlace.transform.position));

        sensor.AddObservation(parkingPlace.GetComponent<ParkingPlace>().numBeaconInPlace);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        continuousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float currentTime = timeToPark;
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            AddReward(-2);
        }

        float horizontalInput = actions.ContinuousActions[0];
        currentSteerAngle = maxSteerAngle * horizontalInput;

        float verticalInput = actions.ContinuousActions[1];
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        isBreaking = actions.ContinuousActions[2] > 0.5f;
        currentBreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();

        HandleSteering();
        UpdateWheels();

        if (!isParked && IsParkedCorrectly())
        {
            isParked = true;
            AddReward(100f);
            ChangeParkingZoneColor(Color.green);
            EndEpisode();
        }
        if (parkingPlace.GetComponent<ParkingPlace>().numBeaconInPlace == numBeaconRequired)
        {
            AddReward(10f);
            ChangeParkingZoneColor(Color.green);
            EndEpisode();
        }
        else
        {
            ChangeParkingZoneColor(Color.red);
        }
    }

    private bool IsParkedCorrectly()
    {
        // Vérifier si chaque roue touche le collider de la place de parking
        bool isCorrectlyParked = true;
        WheelCollider[] wheelColliders = { frontLeftWheelCollider, frontRightWheelCollider, rearLeftWheelCollider, rearRightWheelCollider };
        Transform[] wheelTransforms = { frontLeftWheelTransform, frontRightWheelTransform, rearLeftWheelTransform, rearRightWheelTransform };

        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if (wheelColliders[i] == null || wheelTransforms[i] == null)
            {
                Debug.LogError("Missing reference to WheelCollider or WheelTransform.");
                return false;
            }

            RaycastHit hit;
            Vector3 rayStart = wheelTransforms[i].position;
            Vector3 rayDirection = -wheelTransforms[i].up;

            if (!Physics.Raycast(rayStart, rayDirection, out hit, wheelColliders[i].radius + 0.1f, 1 << parkingZoneLayer))
            {
                isCorrectlyParked = false;
                break;
            }
        }

        if (!isCorrectlyParked)
        {
            return false;
        }

        // Vérifier l'alignement et la distance par rapport à la place de parking
        float dotProduct = Vector3.Dot(transform.forward, parkingPlace.transform.forward);
        if (Mathf.Abs(dotProduct) < 0.9f)
        {
            return false;
        }
        if (Vector3.Distance(transform.position, parkingPlace.transform.position) > parkingThreshold)
        {
            return false;
        }
        return true;
    }

    private void ChangeParkingZoneColor(Color newColor)
    {
        Material parkingZoneMaterial = parkingPlace.GetComponent<Renderer>().material;
        parkingZoneMaterial.color = newColor;
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Border"))
        {
            AddReward(-5f);
            EndEpisode();
        }
    }
}
