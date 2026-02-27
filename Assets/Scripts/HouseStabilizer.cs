using UnityEngine;

/// <summary>
/// Keeps the house/building upright using Physics forces (Torque) instead of hard rotation.
/// This allows for more natural movement and collisions.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HouseStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    [Tooltip("How strongly the house tries to stay upright.")]
    [SerializeField] float stabilityForce = 50f;
    [Tooltip("Damping to prevent oscillation (wobble).")]
    [SerializeField] float damping = 5f;
    
    [Header("Optional Tilt")]
    [Tooltip("If you want the house to tilt slightly with the pole/parent.")]
    [SerializeField][Range(0f, 1f)] float tiltInfluence = 0.2f;
    
    private Rigidbody2D rb;
    private Transform poleTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Get parent (pole) transform
        if (transform.parent != null)
        {
            poleTransform = transform.parent;
        }
    }

    void FixedUpdate()
    {
        Stabilize();
    }

    void Stabilize()
    {
        float targetAngle = 0f;

        // Calculate target angle based on parent/pole if desired
        if (poleTransform != null && tiltInfluence > 0)
        {
            float poleAngle = poleTransform.rotation.eulerAngles.z;
            // Normalize angle to -180 to 180
            if (poleAngle > 180) poleAngle -= 360;
            targetAngle = poleAngle * tiltInfluence;
        }

        // Current angle
        float currentAngle = rb.rotation;
        
        // Calculate error (difference between desired and current)
        float angleError = Mathf.DeltaAngle(currentAngle, targetAngle);

        // PID Controller (Proportional - Derivative)
        // Torque = (Error * P) - (AngularVelocity * D)
        // We convert angle to radians for physics calculation if needed, but simple scaling works too.
        
        float torque = (angleError * stabilityForce) - (rb.angularVelocity * damping);
        
        rb.AddTorque(torque * Mathf.Deg2Rad); // Scaling by Deg2Rad often helps keep numbers manageable
    }
}
