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

    public GameObject wheelGO;
    public GameObject throttleGO;
    public GameObject gearGO;
    public GameObject ignition1GO;
    public Gear gear;
    public Throttle throttle;
    public Wheel wheel;

    public IgnitionKey1 ignitionKey;
    public bool IgnitionOn { get; set; }
    public BoatController boatController { get; set; }

    private void Start()
    {
        ignitionKey = ignition1GO.GetComponent<IgnitionKey1>();
        IgnitionOn = false;

        gear = gearGO.GetComponent<Gear>();
        throttle = throttleGO.GetComponent<Throttle>();
        wheel = wheelGO.GetComponent<Wheel>();
    }

    private void Update()
    {
        CheckIgnition();

        if (!IgnitionOn)
            return;

        MoveBoat(); // Apllies movement to boat based on boat variables
    }

    private void CheckIgnition()
    {
        if (!ignitionKey.IsOn() && ignitionKey.IsIn())
        {
            IgnitionOn = true;
        }
        else
        {
            IgnitionOn = false;
        }
    }

    private void MoveBoat()
    {
        // Determine direction based on current gear
        float directionMultiplier = gear.currentGear switch
        {
            Gear.GearState.Drive => 1f,
            Gear.GearState.Reverse => -1f,
            _ => 0f // Neutral
        };

        Debug.Log($"Current Gear: {gear.currentGear}, Direction Multiplier: {directionMultiplier}");

        if (directionMultiplier == 0f)
            return; // Don't move if in Neutral

        // Get throttle force
        float throttleForce = throttle._throttleValue;

        Debug.Log($"Throttle Value: {throttleForce}");

        // Get steering direction based on wheel angle
        float steeringAngle = wheel.GetAngle(); // In degrees
        Quaternion rotation = Quaternion.Euler(0f, steeringAngle, 0f);
        Vector3 forwardDirection = rotation * transform.forward;

        Debug.Log($"Steering Angle: {steeringAngle}, Forward Direction: {forwardDirection}");

        // Calculate and apply movement
        Vector3 movement = forwardDirection * throttleForce * directionMultiplier;
        transform.position += movement * Time.deltaTime;

        Debug.Log($"Boat Position: {transform.position}, Movement: {movement}");

    }
}
