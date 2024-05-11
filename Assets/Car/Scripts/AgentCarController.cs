using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentController : Agent
{
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

    [Header("Run Settings")]
    [SerializeField] private float timer = 40f;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(GetComponent<Rigidbody>().angularVelocity);
        sensor.AddObservation(Vector3.Distance(transform.position, parkingPlace.transform.position));
        sensor.AddObservation(Vector3.Distance(transform.position, border.position));
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

        AddReward(-0.1f);

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            AddReward(-10f);
            EndEpisode();
        }

        if (IsCarInsideParkingZone())
        {
            AddReward(10f);
            EndEpisode();
        }
    }

    private bool IsCarInsideParkingZone()
    {
        Transform bodyCar = transform.Find("Body");
        Collider bodyCarCollider = bodyCar.GetComponent<Collider>();

        int layerMask = 1 << parkingPlace.GetComponent<MeshCollider>().gameObject.layer;
        Collider[] colliders = Physics.OverlapBox(bodyCarCollider.transform.position, bodyCarCollider.bounds.extents, bodyCarCollider.transform.rotation, layerMask, QueryTriggerInteraction.Ignore);

        return colliders.Length > 1;
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
        }
    }
}
