using FishNet.Object;
using UnityEngine;

// Runs local player behavior in two layers:
// locomotion for movement/piloting, overlay for UI states like inventory.
public class PlayerStateMachine : NetworkBehaviour
{
    public PlayerState CurrentState => CurrentLocomotionState;
    public PlayerState CurrentLocomotionState { get; private set; }
    public PlayerState CurrentOverlayState { get; private set; }

    [Header("Status")]
    [SerializeField] private string locomotionStateName;
    [SerializeField] private string overlayStateName;

    public void Initialize(PlayerState startingState)
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState = startingState;
        locomotionStateName = startingState.GetType().Name;
        startingState.Enter();
    }

    public void SwitchState(PlayerState newState)
    {
        SwitchLocomotionState(newState);
    }

    public void SwitchLocomotionState(PlayerState newState)
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.Exit();
        CurrentLocomotionState = newState;
        locomotionStateName = newState.GetType().Name;
        newState.Enter();
    }

    public void SetOverlayState(PlayerState newState)
    {
        if (!IsOwner)
            return;

        // Only one overlay can be active at a time, but it can coexist with locomotion.
        if (CurrentOverlayState?.GetType() == newState.GetType())
            return;

        CurrentOverlayState?.Exit();
        CurrentOverlayState = newState;
        overlayStateName = newState.GetType().Name;
        newState.Enter();
    }

    public void ClearOverlayState(PlayerState state)
    {
        if (!IsOwner || CurrentOverlayState != state)
            return;

        CurrentOverlayState.Exit();
        CurrentOverlayState = null;
        overlayStateName = string.Empty;
    }

    public void Update()
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.Update();
        CurrentOverlayState?.Update();
    }

    public void LateUpdate()
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.LateUpdate();
        CurrentOverlayState?.LateUpdate();
    }
}
