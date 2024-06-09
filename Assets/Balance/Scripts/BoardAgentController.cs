using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(DecisionRequester))]

public class BoardAgentController : Agent
{
    [Header("References")]
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject secondBall;
    private Rigidbody ballRigidBody;
    private Rigidbody secondBallRigidBody;

    [Header("Gameplay properties")]
    [SerializeField] private float rotateBoardSpeed = 100f;
    [SerializeField] private float isFallenThreshold = -0.5f;
    [SerializeField] private float timeLimit = 20f;
    [SerializeField] private float maxInitialBoardAngle = 80f;

    private Vector3 initialBallPosition;
    private Vector3 initialSecondBallPosition;
    private float episodeTime;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        secondBallRigidBody = secondBall.GetComponent<Rigidbody>();
        episodeTime = 0f;
        initialBallPosition = ballRigidBody.transform.localPosition;
        initialSecondBallPosition = secondBallRigidBody.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        float xBoardRotation = Random.Range(0f, maxInitialBoardAngle);
        float yBoardRotation = Random.Range(0f, 360f);
        float zBoardRotation = Random.Range(0f, maxInitialBoardAngle);
        gameObject.transform.localEulerAngles = new Vector3(xBoardRotation, yBoardRotation, zBoardRotation);

        ball.transform.localPosition = initialBallPosition;
        secondBall.transform.localPosition = initialSecondBallPosition;

        ballRigidBody.velocity = Vector3.zero;
        secondBallRigidBody.velocity = Vector3.zero;

        episodeTime = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(gameObject.transform.localEulerAngles);
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(ballRigidBody.velocity);
        sensor.AddObservation(secondBall.transform.localPosition);
        sensor.AddObservation(secondBallRigidBody.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float xRotateRate = actions.ContinuousActions[0];
        float zRotateRate = actions.ContinuousActions[1];

        xRotateRate = Mathf.Clamp(xRotateRate, -1f, 1f);
        zRotateRate = Mathf.Clamp(zRotateRate, -1f, 1f);

        float xRotation = xRotateRate * rotateBoardSpeed * Time.deltaTime;
        float zRotation = zRotateRate * rotateBoardSpeed * Time.deltaTime;

        gameObject.transform.Rotate(xRotation, 0f, zRotation, Space.Self);

        episodeTime += Time.deltaTime;

        if (ball.transform.localPosition.y <= isFallenThreshold || secondBall.transform.localPosition.y <= isFallenThreshold)
        {
            AddReward(-5f);
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
