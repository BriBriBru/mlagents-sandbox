using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentController : Agent
{
    private float horizontalInput;
    private float verticalInput;
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

    private void FixedUpdate()
    {
        // GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect the car's velocity, angular velocity, and distance to the parking zone and border
        sensor.AddObservation(GetComponent<Rigidbody>().velocity);
        sensor.AddObservation(GetComponent<Rigidbody>().angularVelocity);
        sensor.AddObservation(Vector3.Distance(transform.position, parkingPlace.transform.position));
        sensor.AddObservation(Vector3.Distance(transform.position, border.position));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        Debug.Log("ok");
        // Steering Input
        continousActions[0] = Input.GetAxis("Horizontal");
        horizontalInput = Input.GetAxis("Horizontal");


        // Acceleration Input
        continousActions[1] = Input.GetAxis("Vertical");
        verticalInput = Input.GetAxis("Vertical");

        // Breaking Input
        continousActions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        isBreaking = Input.GetKey(KeyCode.Space);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        // Steering Input
        float horizontalInput = actions.ContinuousActions[0];
        currentSteerAngle = maxSteerAngle * horizontalInput;

        // Acceleration Input
        float verticalInput = actions.ContinuousActions[1];
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        // Breaking Input
        isBreaking = actions.ContinuousActions[2] > 0.5f;
        currentBreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();

        HandleSteering();
        UpdateWheels();

        AddReward(-0.1f); // Small negative reward to encourage the agent to complete the task as quickly as possible

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            AddReward(-10f); // Penalty for not parking within 40 seconds
            EndEpisode();
        }

        // Check if the car is inside the parking zone
        if (IsCarInsideParkingZone())
        {
            AddReward(10f); // Reward for parking successfully
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Border"))
        {
            AddReward(-5f);
            EndEpisode();
        }
    }

    private bool IsCarInsideParkingZone()
    {
        // Create a temporary collider for the car's bounds
        Collider carCollider = new BoxCollider();
        carCollider.transform.position = transform.position;
        carCollider.transform.rotation = transform.rotation;
        carCollider.isTrigger = true;

        // Calculate the car's bounds and set the temporary collider's size
        Vector3 carSize = new Vector3(
            Mathf.Abs(transform.lossyScale.x) * GetComponent<Renderer>().bounds.size.x,
            Mathf.Abs(transform.lossyScale.y) * GetComponent<Renderer>().bounds.size.y,
            Mathf.Abs(transform.lossyScale.z) * GetComponent<Renderer>().bounds.size.z
        );
        carCollider.transform.localScale = carSize;
        ((BoxCollider)carCollider).size = carSize;

        // Check for collisions between the temporary collider and the parking zone
        int layerMask = 1 << parkingPlace.GetComponent<MeshCollider>().gameObject.layer;
        Collider[] colliders = Physics.OverlapBox(carCollider.transform.position, carCollider.transform.localScale * 0.5f, carCollider.transform.rotation, layerMask, QueryTriggerInteraction.Ignore);

        // Remove the temporary collider and return the result
        Destroy(carCollider.gameObject);
        return colliders.Length > 0;
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentBreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
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
        currentSteerAngle = maxSteerAngle * horizontalInput;
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
}
