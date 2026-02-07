using UnityEngine;

/// <summary>
/// Keeps the house/building upright while its parent (pole) rotates.
/// Attach this to the Square child object of the Pole.
/// </summary>
public class HouseStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    [SerializeField] float stabilizationSpeed = 10f;
    [SerializeField] bool keepCompletelyUpright = true;
    
    [Header("Optional Tilt")]
    [Tooltip("If you want the house to tilt slightly with the pole, set this to a value like 0.2")]
    [SerializeField][Range(0f, 1f)] float tiltInfluence = 0f;
    
    private Transform poleTransform;
    private Quaternion uprightRotation = Quaternion.identity;

    void Start()
    {
        // Get parent (pole) transform
        if (transform.parent != null)
        {
            poleTransform = transform.parent;
        }
        
        // Store the initial upright rotation
        uprightRotation = Quaternion.Euler(0, 0, 0);
    }

    void LateUpdate()
    {
        if (keepCompletelyUpright)
        {
            // Keep completely upright (counter-rotate against parent)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                uprightRotation, 
                Time.deltaTime * stabilizationSpeed
            );
        }
        else if (poleTransform != null && tiltInfluence > 0)
        {
            // Partially follow pole rotation
            float poleAngle = poleTransform.rotation.eulerAngles.z;
            if (poleAngle > 180) poleAngle -= 360;
            
            Quaternion targetRotation = Quaternion.Euler(0, 0, poleAngle * tiltInfluence);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * stabilizationSpeed
            );
        }
    }
}
