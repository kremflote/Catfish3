using FishNet.Object;
using FishNet.Connection;
using UnityEngine;

public class Gear : MovableObject
{
    public float leverMoveSpeed;

    private Vector3 basePosition;
    private Vector3 targetPosition; // when gears are switched this updates

    // gears
    public GearState currentGear;
    public GearState lastGear;

    // positions of gears
    Vector3 drivePos;
    Vector3 neutralPos;
    Vector3 reversePos;

    // offsets
    private float drivePositionOffset;
    private float reversePositionOffset;

    // sensitivity for mouse when moving gears
    float mouseSensitivity = 0.0007f;

    public bool moved = false; // used to check if lever has been moved by player
    [SerializeField] private Boat boat;

    // Sets up lever positions for Drive, Neutral, and Reverse relative to the starting transform.
    private void Start()
    {
        if (boat == null)
            boat = GetComponentInParent<Boat>();

        basePosition = transform.localPosition;
        currentGear = GearState.Neutral;
        lastGear = currentGear;
        targetPosition = basePosition;

        // set offsets and speed
        drivePositionOffset = 0.1f;
        reversePositionOffset = -0.1f;
        leverMoveSpeed = 5f;


        // sets default gear positions based on gameobjects' position
        drivePos = basePosition + (transform.forward * drivePositionOffset);
        reversePos = basePosition + (transform.forward * reversePositionOffset);
        neutralPos = basePosition;

    }

    // Keeps the lever moving toward the selected gear notch after the player releases it.
    private void Update()
    {
        UpdateGearTarget();
        MoveLeverToTarget();
    }

    // Commits the nearest gear notch after the player drags/releases the lever.
    public void UpdateGearState()
    {
        if (IsServerInitialized)
        {
            UpdateGearStateLocal();
            SyncGearObserversRpc(transform.localPosition, currentGear, moved);
            return;
        }

        UpdateGearStateLocal();

        if (IsClientInitialized)
            UpdateGearStateServerRpc();
    }

    // Server commits gear changes and broadcasts the resulting lever state.
    [ServerRpc(RequireOwnership = false)]
    private void UpdateGearStateServerRpc()
    {
        UpdateGearStateLocal();
        SyncGearObserversRpc(transform.localPosition, currentGear, moved);
    }

    // Chooses the nearest gear position based on the lever's current local position.
    private void UpdateGearStateLocal()
    {
        Vector3 leverPosition = transform.localPosition;

        float distToDrive = Vector3.Distance(leverPosition, drivePos);
        float distToNeutral = Vector3.Distance(leverPosition, neutralPos);
        float distToReverse = Vector3.Distance(leverPosition, reversePos);

        float minDistance = Mathf.Min(distToDrive, distToNeutral, distToReverse);

        if (minDistance == distToDrive && currentGear != GearState.Drive)
        {
            ChangeGear(GearState.Drive);
        }
        else if (minDistance == distToReverse && currentGear != GearState.Reverse)
        {
            ChangeGear(GearState.Reverse);
        }
        else if (minDistance == distToNeutral && currentGear != GearState.Neutral)
        {
            ChangeGear(GearState.Neutral);
        }

    }

    // Smoothly animates the lever to the target notch after a gear change.
    public void MoveLeverToTarget()
    {
        if (!moved)
        {
            // No change in gear, no need to move
            return;
        }
        // Smoothly move the lever toward the target position
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            leverMoveSpeed * Time.deltaTime
        );

        // Check if movement is complete
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
        {
            moved = false; // Movement complete
        }
    }

    // Updates the target notch when currentGear changes.
    public void UpdateGearTarget()
    {
        // If gear has changed, update the target position
        if (currentGear != lastGear)
        {
            UpdateTargetPosition(currentGear);
            lastGear = currentGear;
        }
    }
    public enum GearState
    {
        Drive,
        Reverse,
        Neutral
    }
    // Changes the logical gear; UpdateGearTarget moves the lever visual afterward.
    public void ChangeGear(GearState state)
    {
        currentGear = state;
    }

    // Local/test overload for moving the lever without a known network pilot.
    public void Move(float mouseDeltaY)
    {
        Move(mouseDeltaY, -1);
    }

    // Moves the gear lever from mouse drag, validating pilot authority before applying it.
    public void Move(float mouseDeltaY, int pilotClientId)
    {
        if (!CanPilotMove(pilotClientId))
            return;

        if (IsServerInitialized)
        {
            MoveLocal(mouseDeltaY);
            SyncGearObserversRpc(transform.localPosition, currentGear, moved);
            return;
        }

        MoveLocal(mouseDeltaY);

        if (IsClientInitialized)
            MoveServerRpc(mouseDeltaY, pilotClientId);
    }

    // Server receives gear lever drag and syncs approved lever state to observers.
    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(float mouseDeltaY, int pilotClientId, NetworkConnection conn = null)
    {
        int requesterId = conn != null && conn.IsValid ? conn.ClientId : pilotClientId;
        if (!CanPilotMove(requesterId))
            return;

        MoveLocal(mouseDeltaY);
        SyncGearObserversRpc(transform.localPosition, currentGear, moved);
    }

    // Prevents non-pilots from moving this control when a boat has an active pilot.
    private bool CanPilotMove(int pilotClientId)
    {
        return boat == null || boat.IsPilot(pilotClientId);
    }

    // FishNet sends the server-approved gear state to non-server clients.
    [ObserversRpc(ExcludeServer = true)]
    private void SyncGearObserversRpc(Vector3 localPosition, GearState gearState, bool isMoved)
    {
        transform.localPosition = localPosition;
        currentGear = gearState;
        lastGear = gearState;
        moved = isMoved;
        UpdateTargetPosition(currentGear);
    }

    // Converts mouse delta into a clamped lever position along the lever's forward axis.
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
        float clampedDistance = Mathf.Clamp(projectedDistance, reversePositionOffset, drivePositionOffset);

        // Recalculate final clamped position
        Vector3 clampedPosition = basePosition + (forwardDir * clampedDistance);
        transform.localPosition = clampedPosition;
    }

    // Steps the gear up one notch for future keyboard/controller support.
    public void UpGear()
    {
        if (currentGear == GearState.Neutral)
        {
            ChangeGear(GearState.Drive);
        }
        else if (currentGear == GearState.Drive)
        {
            // do nothing
        }
        else if (currentGear == GearState.Reverse)
        {
            ChangeGear(GearState.Neutral);
        }
    }

    // Steps the gear down one notch for future keyboard/controller support.
    public void DownGear()
    {
        if (currentGear == GearState.Neutral)
        {
            ChangeGear(GearState.Reverse);
        }
        else if (currentGear == GearState.Drive)
        {
            ChangeGear(GearState.Neutral);
        }
        else if (currentGear == GearState.Reverse)
        {
            // do nothing
        }
    }

    // Maps a logical gear state to the lever's target local position.
    private void UpdateTargetPosition(GearState state)
    {
        switch (state)
        {
            case GearState.Drive:
                targetPosition = basePosition + (transform.forward * drivePositionOffset);
                break;
            case GearState.Reverse:
                targetPosition = basePosition + (transform.forward * reversePositionOffset);
                break;
            case GearState.Neutral:
                targetPosition = basePosition;
                break;
        }
    }
}
