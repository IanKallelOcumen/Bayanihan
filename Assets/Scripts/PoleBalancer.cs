using UnityEngine;

/// <summary>
/// Balances a pole between two cars/points based on their height difference.
/// Uses Physics rotation if a Rigidbody2D is attached, otherwise uses Transform rotation.
/// </summary>
public class PoleBalancer : MonoBehaviour
{
    [Header("Car References")]
    [SerializeField] Transform carLeft;
    [SerializeField] Transform carRight;
    
    [Header("Balance Settings")]
    [SerializeField] float maxAngle = 30f;
    [SerializeField] float heightSensitivity = 20f;
    [SerializeField] float smoothTime = 0.1f;
    
    private float currentAngle;
    private float angularVelocity; // For SmoothDamp
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // If we don't have a Rigidbody, we must move in Update/LateUpdate to be smooth visually
        if (rb == null)
        {
            CalculateAndApplyRotation();
        }
    }

    void FixedUpdate()
    {
        // If we DO have a Rigidbody, we must move in FixedUpdate
        if (rb != null)
        {
            CalculateAndApplyRotation();
        }
    }

    void CalculateAndApplyRotation()
    {
        if (carLeft == null || carRight == null) return;

        // Calculate height difference between cars
        // If left car is higher, pole should rotate clockwise (negative Z)
        // If right car is higher, pole should rotate counter-clockwise (positive Z)
        float heightDiff = carRight.position.y - carLeft.position.y;
        
        // Calculate target angle
        // Atan2 could be more precise but linear approx is fine for small angles
        // float angleRad = Mathf.Atan2(heightDiff, distanceBetweenCars);
        float targetAngle = Mathf.Clamp(heightDiff * heightSensitivity, -maxAngle, maxAngle);
        
        // Smooth damp the angle
        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angularVelocity, smoothTime);
        
        // Apply rotation
        if (rb != null)
        {
            rb.MoveRotation(currentAngle);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }
}
