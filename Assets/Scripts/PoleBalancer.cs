using UnityEngine;

public class PoleBalancer : MonoBehaviour
{
    [Header("Car References")]
    [SerializeField] Transform carLeft;
    [SerializeField] Transform carRight;
    
    [Header("Balance Settings")]
    [SerializeField] float maxAngle = 20f;
    [SerializeField] float heightSensitivity = 30f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float smoothTime = 0.15f;
    
    private float currentAngle;
    private float angularVelocity;

    void Update()
    {
        BalancePole();
    }

    void BalancePole()
    {
        // Calculate height difference between cars
        float heightDiff = carLeft.position.y - carRight.position.y;
        
        // Calculate target angle with damping for smoother response
        float targetAngle = Mathf.Clamp(heightDiff * heightSensitivity, -maxAngle, maxAngle);
        
        // Smooth damp the angle for natural physics feel
        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref angularVelocity, smoothTime);
        
        // Apply smooth rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, -currentAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}