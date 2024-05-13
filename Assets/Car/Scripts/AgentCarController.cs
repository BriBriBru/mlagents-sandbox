using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentController : Agent
{
    private Vector3 initialPosition;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isBreaking;

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
    [SerializeField] private short numBeaconRequired;

    public override void Initialize()
    {
        initialPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = initialPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(parkingPlace.transform.localPosition);
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

        if (parkingPlace.GetComponent<ParkingPlace>().numBeaconInPlace == numBeaconRequired)
        {
            AddReward(20f);
            ChangeParkingZoneColor(Color.green);
            EndEpisode();
        }
        else
        {
            ChangeParkingZoneColor(Color.red);
        }
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
