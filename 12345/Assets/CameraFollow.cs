using UnityEngine;

// Simple smooth camera follow for the rolling ball sample.
public class CameraFollow : MonoBehaviour
{
    [Tooltip("Target transform (the player ball)")]
    public Transform target;
    [Tooltip("Offset from the target in world space")]
    public Vector3 offset = new Vector3(0f, 6f, -8f);
    [Tooltip("How quickly the camera follows (larger = snappier)")]
    public float smoothSpeed = 8f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}

