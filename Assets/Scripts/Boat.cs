using UnityEngine;
using FishNet.Object;
using StarterAssets;
using System;

public class Boat : NetworkBehaviour, IDrivable
{
    [Header("Boat")]
    [Tooltip("Move speed minimum")]
    public float MinSpeed = 1.0f;
    [Tooltip("Move speed maximum")]
    public float MaxSpeed = 40.0f;
    [Tooltip("Rotation speed of the boat")]
    public float RotationSpeed = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public GameObject wheel;
    public GameObject throttle;
    public GameObject gear;
    public GameObject ignition1;

    public IgnitionKey1 ignitionKey;
    public bool IsKeyInserted { get; set; }
    public BoatController boatController { get; set; }

    private void Start()
    {
        ignitionKey = ignition1.GetComponent<IgnitionKey1>();
        IsKeyInserted = false;
    }

    private void MoveBoat(float move, float turn)
    {


    }
    public void InsertKey()
    {
        throw new System.NotImplementedException();
    }
    public void EnterPilotMode()
    {
        throw new System.NotImplementedException();
    }
    public void ExitPilotMode()
    {
        throw new System.NotImplementedException();
    }
}
