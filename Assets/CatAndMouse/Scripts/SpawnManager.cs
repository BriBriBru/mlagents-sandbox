using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : Singleton<SpawnManager>
{
    private Transform[] spawns;
    private List<int> spawnAlreadyUsed;

    void Start()
    {
        spawns = gameObject.GetComponentsInChildren<Transform>();
        spawnAlreadyUsed = new List<int>();
    }

    public void RandomSpawn(GameObject objectToSpawn)
    {
        if (spawnAlreadyUsed.Count == spawns.Length)
        {
            spawnAlreadyUsed.Clear();
        }

        List<int> availableSpawns = Enumerable.Range(0, spawns.Length).Except(spawnAlreadyUsed).ToList();
        int random = UnityEngine.Random.Range(0, availableSpawns.Count);
        int randomIndex = availableSpawns[random];

        spawnAlreadyUsed.Add(randomIndex);
        float x = spawns[randomIndex].localPosition.x;
        float y = objectToSpawn.transform.localPosition.y;
        float z = spawns[randomIndex].localPosition.z;

        objectToSpawn.transform.localPosition = new Vector3(x, y, z);
    }
}
