using FishNet.Object;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; }

    [Header("Status")]
    [SerializeField] private string currentStateName;

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        currentStateName = startingState.GetType().Name;
        startingState.Enter();
    }

    public void SwitchState(PlayerState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        currentStateName = newState.GetType().Name;
        newState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    public void LateUpdate()
    {
        CurrentState?.LateUpdate();
    }
}
