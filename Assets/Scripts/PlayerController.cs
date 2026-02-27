using UnityEngine;

/// <summary>
/// Handles player vehicle physics, input processing, and movement logic.
/// Supports both single car and dual-car control schemes.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(WheelJoint2D))]
[RequireComponent(typeof(WheelJoint2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Required components")]
    [SerializeField] private Rigidbody2D carRigidbody;
    [SerializeField] private Wheel driveWheel;
    [SerializeField] private Wheel secondWheel;

    [Header("Rotation")]
    [Tooltip("Rotation speed when the car is in the air.")]
    [SerializeField] private float onAirRotationSpeed = 8f;
    [Tooltip("Rotation speed when the car is on the ground.")]
    [SerializeField] private float onGroundRotationSpeed = 1f;

    private float brakeInput;
    private float gasInput;
    private float finalInput;

    private float brakeRawInput;
    private float gasRawInput;
    private float finalRawInput;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        
        // Cache Rigidbody if not assigned
        if (carRigidbody == null) carRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        TakeInput();
    }

    void FixedUpdate()
    {
        if (gameManager == null) return;
        if (brakeRawInput == 1f && carRigidbody.linearVelocity.x > 3f)
        {
            Stop();
        }
        else if (brakeRawInput == 0f && gasRawInput == 0f || !gameManager.IsFuel())
        {
            Idle();
        }
        else
        {
            Move();
            Rotate();
        }
    }

    void TakeInput()
    {
        brakeInput = TouchInput.GetDampedBrakeInput();
        gasInput = -TouchInput.GetDampedGasInput();
        brakeRawInput = TouchInput.GetRawBrakeInput();
        gasRawInput = -TouchInput.GetRawGasInput();

        finalInput = brakeRawInput > 0f ? brakeInput : gasInput;
        finalRawInput = brakeRawInput > 0f ? brakeRawInput : gasRawInput;
    }

    void Idle()
    {
        driveWheel.Idle();
        secondWheel.Idle();
    }

    void Stop()
    {
        driveWheel.Stop();
        secondWheel.Stop();
    }

    void Move()
    {
        driveWheel.Move(finalInput);
        secondWheel.Idle();
    }

    // PUBLIC overload for external callers (DualCarController)
    public void Move(float input)
    {
        if (gameManager == null || !gameManager.IsFuel()) return;
        driveWheel.Move(input);
        secondWheel.Idle();
    }

    void Rotate()
    {
        if (!OnGround()) carRigidbody.AddTorque(-finalRawInput * onAirRotationSpeed);
        else carRigidbody.AddTorque(-finalRawInput * onGroundRotationSpeed);
    }

    bool OnGround()
    {
        return driveWheel.OnGround() || secondWheel.OnGround();
    }

    public float GetInput()
    {
        return finalInput;
    }

    public void ApplyPhysics(float friction)
    {
        // Apply friction to wheels
        if (driveWheel != null) ApplyFrictionToWheel(driveWheel, friction);
        if (secondWheel != null) ApplyFrictionToWheel(secondWheel, friction);
    }

    void ApplyFrictionToWheel(Wheel wheel, float friction)
    {
        Collider2D col = wheel.GetComponent<Collider2D>();
        if (col != null)
        {
            // Create a copy of the material to avoid modifying the asset
            PhysicsMaterial2D mat = new PhysicsMaterial2D("LevelFriction");
            mat.friction = friction;
            mat.bounciness = 0.2f; // Default bounciness
            col.sharedMaterial = mat;
        }
    }
}