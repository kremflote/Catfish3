using FishNet.Object;
using UnityEngine;

public class Throttle : MovableObject
{
    [Header("Throttle Settings")]
    private float _throttleValue = 0f;       // Current lever position
    public float _throttleApplied = 0f;     // Target lever position
    public float _maxThrottle = 0.10f;

    public float leverMoveSpeed = 1f;            // modifyer for the lever movement animation

    float mouseSensitivity = 0.0007f;

    // offsets
    private float maxPositionOffset;
    private float minPositionOffset;

    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.localPosition; // Store the lever's rest position
        maxPositionOffset = 0.1f;
        minPositionOffset = -0.1f;
    }

    public void Move(float mouseDeltaY)
    {
        Vector3 offset = transform.forward * (mouseDeltaY / 2) * mouseSensitivity;
        Vector3 newPosition = transform.localPosition + offset;

        // Clamp movement along the forward axis only
        Vector3 forwardDir = transform.forward.normalized;
        Vector3 localOffsetFromBase = newPosition - basePosition;

        // Project movement onto forward direction to get signed distance
        float projectedDistance = Vector3.Dot(localOffsetFromBase, forwardDir);

        // Clamp between reverse and drive positions
        float clampedDistance = Mathf.Clamp(projectedDistance, minPositionOffset, maxPositionOffset);

        // Recalculate final clamped position
        Vector3 clampedPosition = basePosition + (forwardDir * clampedDistance);
        transform.localPosition = clampedPosition;
    }

    void Update()
    {
        // do nothing if the throttleValue hasnt changed (significantly)
        if (Mathf.Approximately(_throttleValue, _throttleApplied))
            return;

        // Smoothly move the lever toward the desired position
        _throttleValue = Mathf.MoveTowards(_throttleValue, _throttleApplied, leverMoveSpeed * Time.deltaTime);

        // Convert throttleValue to a local offset from base position
        transform.localPosition = basePosition + (transform.forward * _throttleValue);

        // clamping the throttle value to the maximum throttle
        _throttleApplied = Mathf.Clamp(_throttleApplied, -_maxThrottle, _maxThrottle);

    }
}
