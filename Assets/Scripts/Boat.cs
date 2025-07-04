using UnityEngine;
using FishNet.Object;
using StarterAssets;
using System;

public class Boat : NetworkBehaviour, IDrivable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;

    public GameObject wheel;
    public GameObject throttle;
    public GameObject gear;
    public GameObject ignition1;

    public IgnitionKey1 ignitionKey;
    public bool IsKeyInserted { get; set; } // player går i "pilot mode" når han inserter key, så denne boolen styrer basically om båten blir styrt eller ikke
    public BoatController boatController { get; set; }

    private void Start()
    {
        ignitionKey = ignition1.GetComponent<IgnitionKey1>();
        IsKeyInserted = false;
    }
    void Update()
    {
        if (!IsOwner) return;
        if (ignitionKey.IsIn() == false) return;
    }

    private void MoveBoat(float move, float turn)
    {
        // Time.fixedDeltaTime used for consistent movement on server
        float moveAmount = move * moveSpeed * Time.fixedDeltaTime;
        float turnAmount = turn * turnSpeed * Time.fixedDeltaTime;

        transform.Translate(Vector3.forward * moveAmount);
        transform.Rotate(Vector3.up * turnAmount);
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
