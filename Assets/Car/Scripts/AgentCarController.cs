using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentController : Agent
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isBreaking;
    private float elapsedTime;

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
    [SerializeField] private const float timeLimit = 120f;

    public override void Initialize()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        elapsedTime = 0f; // Reset the timer
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position and rotation
        sensor.AddObservation(transform.localPosition.x / 10f); // Normalize position
        sensor.AddObservation(transform.localPosition.z / 10f); // Normalize position
        sensor.AddObservation(transform.localRotation.y / 360f); // Normalize rotation

        // Parking spot position
        sensor.AddObservation(parkingPlace.transform.localPosition.x / 10f); // Normalize position
        sensor.AddObservation(parkingPlace.transform.localPosition.z / 10f); // Normalize position

        // Number of beacons in place
        sensor.AddObservation(parkingPlace.GetComponent<ParkingPlace>().numBeaconInPlace);

        // Agent's velocity
        sensor.AddObservation(GetComponent<Rigidbody>().velocity.x / 10f); // Normalize velocity
        sensor.AddObservation(GetComponent<Rigidbody>().velocity.z / 10f); // Normalize velocity

        // Distance to the parking spot
        float distanceToParking = Vector3.Distance(transform.position, parkingPlace.transform.position) / 10f; // Normalize distance
        sensor.AddObservation(distanceToParking);
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
        float horizontalInput = actions.ContinuousActions[0];
        currentSteerAngle = maxSteerAngle * horizontalInput;
        float verticalInput = actions.ContinuousActions[1];
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        HandleSteering();
        UpdateWheels();

        isBreaking = actions.ContinuousActions[2] > 0.5f;
        currentBreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();

        // Calculate distance to the parking spot
        float distanceToParking = Vector3.Distance(transform.position, parkingPlace.transform.position);

        // Intermediate reward for being close to the parking spot
        if (distanceToParking < 5f)
        {
            AddReward(0.1f);
        }

        // Intermediate reward for facing the correct direction
        Vector3 directionToParking = (parkingPlace.transform.position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.forward, directionToParking);
        if (dotProduct > 0.9f)
        {
            AddReward(0.1f);
        }

        if (parkingPlace.GetComponent<ParkingPlace>().numBeaconInPlace == numBeaconRequired)
        {
            AddReward(100f);
            ChangeParkingZoneColor(Color.green);
            EndEpisode();
        }
        else
        {
            ChangeParkingZoneColor(Color.red);
        }

        // Update the timer
        elapsedTime += Time.fixedDeltaTime;

        // Apply penalty if the agent does not park within the time limit
        if (elapsedTime > timeLimit)
        {
            AddReward(-50f); // Apply a penalty
            EndEpisode();
        }
    }

    private void ChangeParkingZoneColor(Color newColor)
    {
        Material parkingZoneMaterial = parkingPlace.GetComponent<Renderer>().material;
        parkingZoneMaterial.color = newColor;
    }

    #region Functionnal car
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
    #endregion

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Border"))
        {
            AddReward(-5f);
            EndEpisode();
        }
    }
}
