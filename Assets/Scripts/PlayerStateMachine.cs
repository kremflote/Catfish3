using FishNet.Object;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : NetworkBehaviour
{
    public FirstPersonController firstPersonController;
    public BoatController boatController;
    public PlayerInput PlayerInput;

    private string currentState;
    private enum PlayerState { Walking, Piloting }

    public void SwitchState(string state)
    {
        this.currentState = state;
    }

    private void Start()
    {
        PlayerInput = GetComponent<PlayerInput>();
        // Set the initial state
        SwitchState("Walking");
    }

    private void Update()
    {
        if (!IsOwner) return;

        switch (currentState)
        {
            case "Walking":

                break;
        }
    }

}
