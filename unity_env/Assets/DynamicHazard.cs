using UnityEngine;

public class DynamicHazard : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform droneTransform;
    public GameObject debrisPrefab;

    [Header("Spawning Configuration")]
    public float spawnInterval = 1.5f;
    public float spawnHeightOffset = 10f;
    public float spawnRadius = 5f;
    public float debrisLifetime = 4.0f;

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime && droneTransform != null)
        {
            SpawnDebrisAboveDrone();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnDebrisAboveDrone()
    {
        // Calculate a random offset inside a cylinder above the drone's current position
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = droneTransform.position + new Vector3(
            randomCircle.x,
            spawnHeightOffset,
            randomCircle.y
        );

        // Instantiate hazard
        GameObject debris = Instantiate(debrisPrefab, spawnPosition, Random.rotation);

        // Ensure it is parented to the local arena to keep the hierarchy clean
        debris.transform.parent = this.transform;

        // Memory Management: Prevent heap accumulation
        Destroy(debris, debrisLifetime);
    }
}