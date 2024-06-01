using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MouseController : Agent
{
    [Header("Speed variables")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotateSpeed = 100f;

    [Header("Time")]
    [SerializeField] private float timeToSurvive = 20f;

    private Rigidbody rb;
    private bool gotCheese = false;

    public override void OnEpisodeBegin()
    {
        gotCheese = false;
    }

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRate = actions.ContinuousActions[0];
        float rotateRate = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveSpeed * moveRate * Time.deltaTime);
        
        Vector3 angleForSpeed = new Vector3(0f, rotateSpeed, 0f);
        Quaternion deltaRotation = Quaternion.Euler(angleForSpeed * rotateRate * Time.deltaTime);
        rb.MoveRotation(transform.rotation * deltaRotation);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Vertical");
        continousActions[1] = Input.GetAxisRaw("Horizontal");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cheese"))
        {
            AddReward(10f);
            gotCheese = true;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Exit") && gotCheese)
        {
            AddReward(30f);
            EndEpisode();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Cat"))
        {
            AddReward(-20);
        }
    }
}
