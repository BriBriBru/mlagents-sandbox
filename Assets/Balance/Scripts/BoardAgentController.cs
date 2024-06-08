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
    private float episodeTime;
    private Vector3 initialBallPosition;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        episodeTime = 0f;
        initialBallPosition = ballRigidBody.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        // Board random initial angle
        float xBoardRotation = Random.Range(0f, maxBoardAngle);
        float yBoardRotation = Random.Range(0f, 360f);
        float zBoardRotation = Random.Range(0f, maxBoardAngle);
        gameObject.transform.localEulerAngles = new Vector3(xBoardRotation, yBoardRotation, zBoardRotation);

        // Ball initial position
        ball.transform.localPosition = initialBallPosition;

        // Ball initial velocity
        ballRigidBody.velocity = Vector3.zero;

        // Other
        episodeTime = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(gameObject.transform.localRotation);
        sensor.AddObservation(gameObject.transform.localEulerAngles);
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ballRigidBody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float xRotateRate = actions.ContinuousActions[0];
        float zRotateRate = actions.ContinuousActions[1];

        Vector3 newRotation = gameObject.transform.localEulerAngles;
        newRotation.x += rotateBoardSpeed * xRotateRate * Time.deltaTime;
        newRotation.z += rotateBoardSpeed * zRotateRate * Time.deltaTime;

        newRotation.x = Mathf.Clamp(newRotation.x, -maxBoardAngle, maxBoardAngle);
        newRotation.z = Mathf.Clamp(newRotation.z, -maxBoardAngle, maxBoardAngle);

        gameObject.transform.localEulerAngles = newRotation;

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
