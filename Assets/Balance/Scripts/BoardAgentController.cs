using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(DecisionRequester))]

public class BoardAgentController : Agent
{
    [Header("References")]
    [SerializeField] private GameObject ball;
    private Rigidbody ballRigidBody;

    [Header("Gameplay properties")]
    [SerializeField] private float rotateBoardSpeed = 100f;
    [SerializeField] private float isFallenThreshold = -0.5f;
    [SerializeField] private float timeLimit = 20f;
    [SerializeField] private float maxBoardAngle = 30f;
    private Vector3 initialBallPosition;
    private Quaternion initialBoardRotation;
    private Vector3 currentRotation;
    private float episodeTime;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        initialBallPosition = ballRigidBody.transform.localPosition;
        initialBoardRotation = gameObject.transform.localRotation;
        currentRotation = Vector3.zero;
        episodeTime = 0f;
    }

    public override void OnEpisodeBegin()
    {
        gameObject.transform.localRotation = initialBoardRotation;
        ball.transform.localPosition = initialBallPosition;
        currentRotation = Vector3.zero;
        episodeTime = 0f;

        float x = Random.Range(0.01f, 0.05f);
        float y = Random.Range(0.01f, 0.05f);
        float z = Random.Range(0.01f, 0.05f);
        ballRigidBody.velocity = new Vector3(x, y, z);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.localRotation);
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ballRigidBody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float xRotateRate = actions.ContinuousActions[0];
        float zRotateRate = actions.ContinuousActions[1];

        currentRotation.x += rotateBoardSpeed * xRotateRate * Time.deltaTime;
        currentRotation.z += rotateBoardSpeed * zRotateRate * Time.deltaTime;

        currentRotation.x = Mathf.Clamp(currentRotation.x, -maxBoardAngle, maxBoardAngle);
        currentRotation.z = Mathf.Clamp(currentRotation.z, -maxBoardAngle, maxBoardAngle);

        gameObject.transform.localEulerAngles = currentRotation;

        episodeTime += Time.deltaTime;

        if (ball.transform.localPosition.y <= isFallenThreshold)
        {
            AddReward(-50f);
            ChangeBoardColor(Color.red);
            EndEpisode();
        }

        else
        {
            AddReward(0.1f);
        }

        if (episodeTime >= timeLimit)
        {
            ChangeBoardColor(Color.green);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Vertical");
        continousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    private void ChangeBoardColor(Color newColor)
    {
        Material material = gameObject.GetComponent<Renderer>().material;
        material.color = newColor;
    }
}
