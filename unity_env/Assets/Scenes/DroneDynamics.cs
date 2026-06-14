using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneDynamics : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Flight Parameters")]
    public float thrustMultiplier = 25f;
    public float rotationMultiplier = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 1.5f;
        rb.linearDamping = 1.0f; // Linear air resistance
        rb.angularDamping = 2.0f; // Dampens erratic spinning
    }

    /// <summary>
    /// Translates normalized continuous actions [-1, 1] from the RL policy into physical forces.
    /// </summary>
    public void ApplyControlInputs(float thrust, float pitch, float yaw, float roll)
    {
        // 1. Calculate baseline force required to counteract gravity
        float hoverForce = rb.mass * Mathf.Abs(Physics.gravity.y);

        // 2. Apply vertical thrust vector
        Vector3 verticalThrust = Vector3.up * (hoverForce + (thrust * thrustMultiplier));
        rb.AddRelativeForce(verticalThrust, ForceMode.Force);

        // 3. Apply rotational torques (Pitch: X-axis, Yaw: Y-axis, Roll: Z-axis)
        // Roll is inverted natively in Unity to match right-hand flight dynamics
        Vector3 rotationalTorque = new Vector3(pitch, yaw, -roll) * rotationMultiplier;
        rb.AddRelativeTorque(rotationalTorque, ForceMode.Force);
    }
}