using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class Throttle : MovableObject
{
    [Header("Throttle Settings")]
    public float _throttleValue = 0f;       // Current lever position
    public float _throttleApplied = 0f;     // Target lever position
    public float _maxThrottle = 0.10f;

    public float leverMoveSpeed = 1f;            // modifyer for the lever movement animation

    float mouseSensitivity = 0.0007f;

    // offsets
    private float maxPositionOffset;
    private float minPositionOffset;

    private Vector3 basePosition;
    [SerializeField] private Boat boat;

    private void Start()
    {
        if (boat == null)
            boat = GetComponentInParent<Boat>();

        basePosition = transform.localPosition; // Store the lever's rest position
        maxPositionOffset = 0.1f;
        minPositionOffset = -0.1f;
    }

    public void Move(float mouseDeltaY)
    {
        Move(mouseDeltaY, -1);
    }

    public void Move(float mouseDeltaY, int pilotClientId)
    {
        if (!CanPilotMove(pilotClientId))
            return;

        if (IsServerInitialized)
        {
            MoveLocal(mouseDeltaY);
            SyncThrottleObserversRpc(transform.localPosition, _throttleApplied, _throttleValue);
            return;
        }

        MoveLocal(mouseDeltaY);

        if (IsClientInitialized)
            MoveServerRpc(mouseDeltaY, pilotClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(float mouseDeltaY, int pilotClientId, NetworkConnection conn = null)
    {
        int requesterId = conn != null && conn.IsValid ? conn.ClientId : pilotClientId;
        if (!CanPilotMove(requesterId))
            return;

        MoveLocal(mouseDeltaY);
        SyncThrottleObserversRpc(transform.localPosition, _throttleApplied, _throttleValue);
    }

    private bool CanPilotMove(int pilotClientId)
    {
        return boat == null || boat.IsPilot(pilotClientId);
    }

    [ObserversRpc(ExcludeServer = true)]
    private void SyncThrottleObserversRpc(Vector3 localPosition, float throttleApplied, float throttleValue)
    {
        transform.localPosition = localPosition;
        _throttleApplied = throttleApplied;
        _throttleValue = throttleValue;
    }

    private void MoveLocal(float mouseDeltaY)
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

        UpdateThrottleFromLever();
    }

    void Update()
    {
        UpdateThrottleFromLever();
    }

    private void UpdateThrottleFromLever()
    {

        // Calculate offset from base position
        Vector3 localOffset = transform.localPosition - basePosition;

        // Project onto forward axis to get how far the lever has moved (positive or negative)
        float projectedDistance = Vector3.Dot(localOffset, transform.forward.normalized);

        // Clamp it to ensure it's within valid range
        _throttleApplied = Mathf.Clamp(projectedDistance, -_maxThrottle, _maxThrottle);

        // Apply the calculated throttle to the lever position
        transform.localPosition = basePosition + (transform.forward * _throttleApplied);

        // Sync throttle value
        _throttleValue = _throttleApplied;
    }
}
