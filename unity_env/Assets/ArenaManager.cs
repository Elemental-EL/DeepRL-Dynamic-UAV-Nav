using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : MonoBehaviour
{
    [Header("Arena Bounds (Local Coordinates)")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    [Header("Dynamic Elements")]
    public Transform droneStart;
    public Transform target;
    public List<GameObject> proceduralObstacles;

    [Header("Randomization Parameters")]
    public float safeRadius = 4.0f; // Prevent spawning on drone/target
    public Vector2 scaleRange = new Vector2(1f, 4f);

    // Called by the DroneAgent at the start of every episode.
    public void RandomizeEnvironment()
    {
        // Randomize Target Position
        target.localPosition = GetValidSpawnPosition();

        // Randomize Obstacles (Layout, Rotation, and Size)
        foreach (GameObject obs in proceduralObstacles)
        {
            // 50% chance to disable an obstacle to vary density per episode
            bool isActive = Random.value > 0.5f;
            obs.SetActive(isActive);

            if (isActive)
            {
                obs.transform.localPosition = GetValidSpawnPosition();

                // Randomize scale for diverse gaps and heights
                float randomX = Random.Range(scaleRange.x, scaleRange.y);
                float randomY = Random.Range(scaleRange.x, scaleRange.y * 2f);
                float randomZ = Random.Range(scaleRange.x, scaleRange.y);
                obs.transform.localScale = new Vector3(randomX, randomY, randomZ);

                obs.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }
        }
    }

    public Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = Vector3.zero;
        bool valid = false;
        int attempts = 0;

        while (!valid && attempts < 50)
        {
            spawnPos = new Vector3(Random.Range(minX, maxX), 2f, Random.Range(minZ, maxZ));

            // Check distance against drone start position
            if (Vector3.Distance(spawnPos, droneStart.localPosition) > safeRadius)
            {
                valid = true;
            }
            attempts++;
        }
        return spawnPos;
    }
}