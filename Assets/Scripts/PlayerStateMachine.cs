using FishNet.Object;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }

    public void SwitchState(PlayerState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        newState.Enter();
    }

    public void Update() => CurrentState?.Update();
    public void LateUpdate() => CurrentState?.LateUpdate();
}
