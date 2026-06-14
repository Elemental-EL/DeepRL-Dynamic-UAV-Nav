using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DroneAgent : Agent
{
    private Rigidbody rb;
    private DroneDynamics droneDynamics;

    [Header("Target Parameters")]
    public Transform target; // The survivor / goal
    public Transform arenaCenter; // For bounding out-of-bounds checks

    private float previousDistance;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        droneDynamics = GetComponent<DroneDynamics>();
    }

    // Total Vector Observations explicitly added = 3 + 3 + 4 + 3 = 13 floats.
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Linear Velocity (3 floats) - Helps the network damp velocity to avoid drifting
        sensor.AddObservation(rb.linearVelocity);

        // 2. Angular Velocity (3 floats) - Teaches the network to stop spinning erratically
        sensor.AddObservation(rb.angularVelocity);

        // 3. Orientation Quaternion (4 floats: x, y, z, w) - Drone's attitude in 3D space
        sensor.AddObservation(transform.rotation);

        // 4. Normalized Direction Vector to Target (3 floats)
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        sensor.AddObservation(directionToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous actions array mapping:
        float thrust = actions.ContinuousActions[0]; // Vertical force
        float pitch = actions.ContinuousActions[1]; // X-axis Torque
        float yaw = actions.ContinuousActions[2]; // Y-axis Torque
        float roll = actions.ContinuousActions[3]; // Z-axis Torque

        // Pass actions to the physics engine
        droneDynamics.ApplyControlInputs(thrust, pitch, yaw, roll);

        // Existential Penalty
        // Encourages the drone to reach the target quickly.
        AddReward(-1f / MaxStep);

        // Distance Delta
        float currentDistance = Vector3.Distance(transform.position, target.position);
        float distanceDelta = previousDistance - currentDistance;
        AddReward(distanceDelta * 0.1f); // 0.1f == tuning weight

        // Stability Penalty
        // Penalizes high angular velocity so the drone doesn't spin erratically
        float spinMagnitude = rb.angularVelocity.magnitude;
        AddReward(-spinMagnitude * 0.01f);

        previousDistance = currentDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetKey(KeyCode.Space) ? 1.0f : (Input.GetKey(KeyCode.LeftShift) ? -1.0f : 0.0f);
        continuousActions[1] = Input.GetAxis("Vertical");   // Pitch (W/S)
        continuousActions[2] = Input.GetAxis("Horizontal"); // Yaw (A/D)
        continuousActions[3] = Input.GetAxis("Horizontal"); // Roll (Approximated)
    }

    public override void OnEpisodeBegin()
    {
        // Reset dynamics instantly to prevent residual velocity transfer between episodes
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 2f, 0); // Reset above floor level
        transform.rotation = Quaternion.identity;

        // Randomize target position within the arena boundary
        target.localPosition = new Vector3(Random.Range(-20f, 20f), 2f, Random.Range(-20f, 20f));

        // Initialize distance tracker
        previousDistance = Vector3.Distance(transform.position, target.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If it hits the floor, walls, or debris
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Hazard"))
        {
            SetReward(-1.0f); // Penalty for crashing
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            SetReward(1.0f); // Reward for success
            EndEpisode();
        }
    }
}