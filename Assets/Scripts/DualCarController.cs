using UnityEngine;

public class DualCarController : MonoBehaviour
{
    [Header("Assign both cars")]
    [SerializeField] PlayerController carLeft;
    [SerializeField] PlayerController carRight;

    void FixedUpdate()
    {
        float input = -TouchInput.GetDampedGasInput() + TouchInput.GetDampedBrakeInput();
        carLeft.Move(input);
        carRight.Move(input);
    }
}