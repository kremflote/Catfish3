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

    // Starts the owner-side state machine in its initial locomotion state.
    public void Initialize(PlayerState startingState)
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState = startingState;
        locomotionStateName = startingState.GetType().Name;
        startingState.Enter();
    }

    // Backwards-compatible wrapper for older code that did not distinguish locomotion and overlay states.
    public void SwitchState(PlayerState newState)
    {
        SwitchLocomotionState(newState);
    }

    // Replaces the movement/piloting state while preserving any overlay state such as inventory.
    public void SwitchLocomotionState(PlayerState newState)
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.Exit();
        CurrentLocomotionState = newState;
        locomotionStateName = newState.GetType().Name;
        newState.Enter();
    }

    // Starts a UI-style state that runs alongside locomotion instead of replacing it.
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

    // Removes an overlay only if the caller is still the active overlay.
    public void ClearOverlayState(PlayerState state)
    {
        if (!IsOwner || CurrentOverlayState != state)
            return;

        CurrentOverlayState.Exit();
        CurrentOverlayState = null;
        overlayStateName = string.Empty;
    }

    // Runs per-frame logic for both locomotion and overlay layers.
    public void Update()
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.Update();
        CurrentOverlayState?.Update();
    }

    // Runs late-frame logic such as camera rotation after normal Update has processed input/state.
    public void LateUpdate()
    {
        if (!IsOwner)
            return;

        CurrentLocomotionState?.LateUpdate();
        CurrentOverlayState?.LateUpdate();
    }
}
