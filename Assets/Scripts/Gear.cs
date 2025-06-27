using FishNet.Object;
using UnityEngine;

public class Gear : NetworkBehaviour
{
    
    public float leverMoveSpeed = 1f;            // modifyer for the lever movement animation
    private Vector3 basePosition;
    public GearState currentGear;
    public GearState lastGear; // for testing

    private void Start()
    {
        basePosition = transform.localPosition; // Store the lever's rest position
        currentGear = GearState.Neutral;
    }

    void Update()
    {
        if (currentGear != lastGear)
        {
            // Smoothly move the lever toward the desired position
            ChangeGear(currentGear);
            lastGear = currentGear; // Update last gear to current
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
        switch (state)
        {
            case GearState.Drive:
                transform.localPosition = basePosition + (transform.forward * 0.1f); // Move lever forward for Drive
                break;
            case GearState.Reverse:
                transform.localPosition = basePosition + (transform.forward * -0.1f); // Move lever backward for Reverse
                break;
            case GearState.Neutral:
                transform.localPosition = basePosition; // Reset to neutral position
                break;
        }
    }
}
