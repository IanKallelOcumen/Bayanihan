using UnityEngine;

public class KeepRotation : MonoBehaviour
{
    Quaternion startRotation;

    void Awake()
    {
        startRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = startRotation;
    }
}