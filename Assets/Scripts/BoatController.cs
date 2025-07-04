using System;
using System.Globalization;
using FishNet.Object;
using StarterAssets;
using UnityEngine;

public class BoatController : NetworkBehaviour
{
    public PlayerStateMachine StateMachine { get; private set; }
    public StarterAssetsInputs _input { get; set; }
    public InventoryToggleManager InventoryToggleManager;


    public void Initialize(InventoryToggleManager InventoryToggleManager, PlayerStateMachine StateMachine)
    {
        this.InventoryToggleManager = InventoryToggleManager;
        this.StateMachine = StateMachine;
    }

    private void Update()
    {
        if (!IsOwner) return;
        StateMachine.Update();
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        StateMachine.LateUpdate();
    }

    public void Steer()
    {

    }

    internal object GetPilotPosition()
    {
        throw new NotImplementedException();
    }
}
