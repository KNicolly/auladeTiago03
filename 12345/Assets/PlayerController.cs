using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Force applied to move the ball (units: acceleration)")]
    public float speed = 10f;
    [Tooltip("Maximum horizontal speed (meters/second)")]
    public float maxSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public bool allowJump = true;

    Rigidbody _rb;
    bool _grounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        // sensible defaults for a rolling ball
        // use new property names (linearDamping/angularDamping) where available
        _rb.angularDamping = 0.05f;
        _rb.linearDamping = 0.1f;
        // Do not freeze rotation for a natural rolling ball
    }

    void Update()
    {
        // Jump input handled in Update to not miss the press
        // Use the new Input System APIs. Prefer gamepad if available, otherwise keyboard.
        bool jumpPressed = false;
        if (Gamepad.current != null)
        {
            jumpPressed = Gamepad.current.buttonSouth.wasPressedThisFrame;
        }
        else if (Keyboard.current != null)
        {
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (allowJump && _grounded && jumpPressed)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            _grounded = false;
        }
    }

    void FixedUpdate()
    {
        // Read input. Support both old Input Manager and the new Input System.
        Vector2 move = Vector2.zero;
        // Prefer gamepad left stick; fall back to WASD/arrow keys via Keyboard.
        if (Gamepad.current != null)
        {
            move = Gamepad.current.leftStick.ReadValue();
        }
        else if (Keyboard.current != null)
        {
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            // also support arrow keys
            x += (Keyboard.current.rightArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
            y += (Keyboard.current.upArrowKey.isPressed ? 1f : 0f) - (Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
            move = new Vector2(x, y);
        }

        Vector3 input = new Vector3(move.x, 0f, move.y);
        if (input.sqrMagnitude > 0.0001f)
        {
            // Transform input from local to world space if you want camera-relative movement,
            // but for a simple top-down plane use world axes (or rotate input by camera if desired).
            Vector3 force = input.normalized * speed;
            _rb.AddForce(force, ForceMode.Acceleration);
        }

        // Limit horizontal speed so ball doesn't accelerate indefinitely
        Vector3 vel = _rb.linearVelocity;
        Vector3 horizontal = new Vector3(vel.x, 0f, vel.z);
        if (horizontal.magnitude > maxSpeed)
        {
            Vector3 limited = horizontal.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(limited.x, vel.y, limited.z);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Consider grounded if collision normal has sufficient upward component
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                _grounded = true;
                break;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // Keep grounded true while on a surface
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                _grounded = true;
                return;
            }
        }
        _grounded = false;
    }

    void OnCollisionExit(Collision collision)
    {
        // Use the parameter to avoid unused-parameter warnings from analyzers
        _ = collision;
        _grounded = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Simple pickup handling: objects with tag "Pickup" will be deactivated
        // Note: make sure a "Pickup" tag exists in Tags & Layers, otherwise this will always be false.
        if (other.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false);
            // You can add score logic here (e.g., send message to a GameManager)
        }
    }

    // Editor-friendly helper to show recommended setup
    void Reset()
    {
        // Called when component is first added — give the Rigidbody if present some nice defaults
        var r = GetComponent<Rigidbody>();
        if (r != null)
        {
            r.mass = 1f;
            r.linearDamping = 0.1f;
            r.angularDamping = 0.05f;
            r.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
}
