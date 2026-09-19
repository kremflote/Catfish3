using FishNet.Object;
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

    private void Start()
    {
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

    private void Update()
    {
        UpdateGearTarget();
        MoveLeverToTarget();
    }

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

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGearStateServerRpc()
    {
        UpdateGearStateLocal();
        SyncGearObserversRpc(transform.localPosition, currentGear, moved);
    }

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

        Debug.Log($"Current Gear: {currentGear}, Position: {transform.localPosition}");
    }
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
    public void ChangeGear(GearState state)
    {
        currentGear = state;
    }
    public void Move(float mouseDeltaY)
    {
        if (IsServerInitialized)
        {
            MoveLocal(mouseDeltaY);
            SyncGearObserversRpc(transform.localPosition, currentGear, moved);
            return;
        }

        MoveLocal(mouseDeltaY);

        if (IsClientInitialized)
            MoveServerRpc(mouseDeltaY);
    }

    [ServerRpc(RequireOwnership = false)]
    private void MoveServerRpc(float mouseDeltaY)
    {
        MoveLocal(mouseDeltaY);
        SyncGearObserversRpc(transform.localPosition, currentGear, moved);
    }

    [ObserversRpc(ExcludeServer = true)]
    private void SyncGearObserversRpc(Vector3 localPosition, GearState gearState, bool isMoved)
    {
        transform.localPosition = localPosition;
        currentGear = gearState;
        lastGear = gearState;
        moved = isMoved;
        UpdateTargetPosition(currentGear);
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
        float clampedDistance = Mathf.Clamp(projectedDistance, reversePositionOffset, drivePositionOffset);

        // Recalculate final clamped position
        Vector3 clampedPosition = basePosition + (forwardDir * clampedDistance);
        transform.localPosition = clampedPosition;
    }
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
