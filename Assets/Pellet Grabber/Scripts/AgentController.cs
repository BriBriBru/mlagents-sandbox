using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class AgentController : Agent
{
    [Header("Agent variables")]
    [SerializeField] private float moveSpeed = 3f;
    private Rigidbody rb;

    [Header("Pellet variables")]
    [SerializeField] private List<GameObject> spawnedPelletList = new List<GameObject>();
    public int pelletCount;
    public GameObject food;

    [Header("Environment variables")]
    [SerializeField] private float environmentHeight = 0.25f;
    [SerializeField] private float environmentBorder = 4.75f;
    [SerializeField] private Transform environmentLocation;
    [SerializeField] private GameObject env;
    [SerializeField] private float distanceBetweenObjects = 4.75f;
    private Material envMaterial;

    [Header("Time keeping variables")]
    [SerializeField] private float timeForEpisode;
    private float timeLeft;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, environmentHeight, 0f);
        CreatePellet();
        EpisodeTimeNew();
    }

    private void Update()
    {
        CheckRemainingTime();
    }

    private void CreatePellet()
    {
        if (spawnedPelletList.Count != 0)
        {
            RemovePellet(spawnedPelletList);
        }
        for (int i = 0; i < pelletCount; i++)
        {
            int counter = 0;
            bool isDistanceGood;
            bool alreadyDecremented = false;

            GameObject newPellet = Instantiate(food);
            newPellet.transform.parent = environmentLocation;
            Vector3 pelletLocation = new Vector3(Random.Range(-environmentBorder, environmentBorder), environmentHeight, Random.Range(-environmentBorder, environmentBorder));
            
            if (spawnedPelletList.Count != 0)
            {
                for (int k = 0; k < spawnedPelletList.Count; k++)
                {
                    if (counter < pelletCount)
                    {
                        isDistanceGood = CheckOverLap(pelletLocation, spawnedPelletList[k].transform.localPosition, distanceBetweenObjects);
                        if (!isDistanceGood)
                        {
                            
                            pelletLocation = new Vector3(Random.Range(-environmentBorder, environmentBorder), environmentHeight, Random.Range(-environmentBorder, environmentBorder));
                            k--;
                            alreadyDecremented = true;
                        }

                        isDistanceGood = CheckOverLap(pelletLocation, transform.localPosition, distanceBetweenObjects);
                        if (!isDistanceGood)
                        {
                            pelletLocation = new Vector3(Random.Range(-environmentBorder, environmentBorder), environmentHeight, Random.Range(-environmentBorder, environmentBorder));
                            if (!alreadyDecremented)
                            {
                                k--;
                            }
                        }

                        counter++;
                    }
                    else
                    {
                        k = spawnedPelletList.Count;
                    }
                }
            }
            
            newPellet.transform.localPosition = pelletLocation;
            spawnedPelletList.Add(newPellet);
        }
    }

    private bool CheckOverLap(Vector3 objectToAvoidOverlap, Vector3 alreadyExistingObject, float minDistanceWanted)
    {
        float distanceBetweenObjects = Vector3.Distance(objectToAvoidOverlap, alreadyExistingObject);
        if (distanceBetweenObjects <= minDistanceWanted)
        {
            return true;
        }
        return false;
    }

    private void RemovePellet(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach (GameObject pelletToDelete in toBeDeletedGameObjectList)
        {
            Destroy(pelletToDelete);
        }
        toBeDeletedGameObjectList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxisRaw("Horizontal");
        continousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pellet"))
        {
            spawnedPelletList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            if (spawnedPelletList.Count == 0)
            {
                envMaterial.color = Color.green;
                RemovePellet(spawnedPelletList);
                AddReward(5f);
                EndEpisode();
            }
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            envMaterial.color = Color.red;
            RemovePellet(spawnedPelletList);
            AddReward(-15f);
            EndEpisode();
        }
    }

    private void EpisodeTimeNew()
    {
        timeLeft = Time.time + timeForEpisode;
    }

    private void CheckRemainingTime()
    {
        if (Time.time >= timeLeft)
        {
            envMaterial.color = Color.yellow;
            AddReward(-15f);
            RemovePellet(spawnedPelletList);
            EndEpisode();
        }
    }
}
