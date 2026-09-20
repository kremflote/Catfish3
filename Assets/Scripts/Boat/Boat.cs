using UnityEngine;
using FishNet.Object;
using StarterAssets;
using FishNet.Connection;

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
    private float _steerInput;
    private int currentPilotClientId = -1;

    // Finds boat control parts in the prefab and starts with ignition off.
    private void Start()
    {
        ignitionKey = ignition1GO.GetComponent<IgnitionKey1>();
        IgnitionOn = false;

        gear = gearGO.GetComponent<Gear>();
        throttle = throttleGO.GetComponent<Throttle>();
        wheel = wheelGO.GetComponent<Wheel>();
    }

    // Server-side boat tick: validate ignition and move only when the engine is on.
    private void Update()
    {
        if (!IsServerInitialized && IsClientInitialized)
            return;

        CheckIgnition();

        if (!IgnitionOn)
            return;

        MoveBoat(); // Apllies movement to boat based on boat variables
    }

    // Records which client is currently allowed to control this boat.
    public void SetPilot(int clientId)
    {
        if (clientId < 0)
            return;

        if (IsServerInitialized)
        {
            currentPilotClientId = clientId;
            return;
        }

        if (IsClientInitialized)
            SetPilotServerRpc(clientId);
    }

    // Clears the active pilot if the same client stops piloting.
    public void ClearPilot(int clientId)
    {
        if (clientId < 0)
            return;

        if (IsServerInitialized)
        {
            if (currentPilotClientId == clientId)
                currentPilotClientId = -1;
            return;
        }

        if (IsClientInitialized)
            ClearPilotServerRpc(clientId);
    }

    // Allows unclaimed boats or the current pilot to control steering.
    public bool IsPilot(int clientId)
    {
        return currentPilotClientId < 0 || currentPilotClientId == clientId;
    }

    // Accepts steering input locally or forwards it to the server in networked play.
    public void SetSteeringInput(float steerInput, int pilotClientId)
    {
        steerInput = Mathf.Clamp(steerInput, -1f, 1f);

        if (!IsPilot(pilotClientId))
            return;

        if (IsServerInitialized)
            _steerInput = steerInput;
        else if (IsClientInitialized)
            SetSteeringInputServerRpc(steerInput, pilotClientId);
        else
            _steerInput = steerInput;
    }

    // Server receives pilot claims and trusts the requesting connection when available.
    [ServerRpc(RequireOwnership = false)]
    private void SetPilotServerRpc(int clientId, NetworkConnection conn = null)
    {
        currentPilotClientId = conn != null && conn.IsValid ? conn.ClientId : clientId;
    }

    // Server clears piloting only for the client that currently owns the pilot slot.
    [ServerRpc(RequireOwnership = false)]
    private void ClearPilotServerRpc(int clientId, NetworkConnection conn = null)
    {
        int requesterId = conn != null && conn.IsValid ? conn.ClientId : clientId;

        if (currentPilotClientId == requesterId)
            currentPilotClientId = -1;
    }

    // Server receives steering and ignores it unless it came from the active pilot.
    [ServerRpc(RequireOwnership = false)]
    private void SetSteeringInputServerRpc(float steerInput, int pilotClientId, NetworkConnection conn = null)
    {
        int requesterId = conn != null && conn.IsValid ? conn.ClientId : pilotClientId;
        if (!IsPilot(requesterId))
            return;

        _steerInput = Mathf.Clamp(steerInput, -1f, 1f);
    }

    // Derives engine state from the interactable ignition key.
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

    // Applies simple prototype boat movement from gear, throttle, and steering wheel angle.
    private void MoveBoat()
    {
        // Determine direction based on current gear
        float directionMultiplier = gear.currentGear switch
        {
            Gear.GearState.Drive => 1f,
            Gear.GearState.Reverse => -1f,
            _ => 0f // Neutral
        };

        if (directionMultiplier == 0f)
            return; // Don't move if in Neutral

        // Get throttle force
        float throttleForce = throttle._throttleValue;

        // Get steering direction based on wheel angle
        if (wheel != null)
            wheel.AddAngle(_steerInput);

        float steeringAngle = wheel != null ? wheel.GetAngle() : 0f; // In degrees
        Quaternion rotation = Quaternion.Euler(0f, steeringAngle, 0f);
        Vector3 forwardDirection = rotation * transform.forward;

        // Calculate and apply movement
        Vector3 movement = forwardDirection * throttleForce * directionMultiplier;
        transform.position += movement * Time.deltaTime;
    }
}
