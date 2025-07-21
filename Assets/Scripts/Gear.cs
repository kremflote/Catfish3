using FishNet.Object;
using UnityEngine;

public class Gear : InteractableObject
{
    public float leverMoveSpeed = 5f; // Speed at which the lever moves
    private Vector3 basePosition;
    private Vector3 targetPosition;
    public GearState currentGear;
    public GearState lastGear;

    private void Start()
    {
        basePosition = transform.localPosition;
        currentGear = GearState.Neutral;
        lastGear = currentGear;
        targetPosition = basePosition;
    }

    private void Update()
    {
        // Smoothly move the lever toward the target position
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            leverMoveSpeed * Time.deltaTime
        );

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

    private void UpdateTargetPosition(GearState state)
    {
        switch (state)
        {
            case GearState.Drive:
                targetPosition = basePosition + (transform.forward * 0.1f);
                break;
            case GearState.Reverse:
                targetPosition = basePosition + (transform.forward * -0.1f);
                break;
            case GearState.Neutral:
                targetPosition = basePosition;
                break;
        }
    }
}
