using UnityEngine;
using FishNet.Object;

public class Boat : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 100f;

    private float moveInput;
    private float turnInput;

    private bool turnedOn = false;

    void Update()
    {
        if (!IsOwner) return;

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        // Send input to the server for authoritative movement
        SendInputToServer(moveInput, turnInput);
    }

    [ServerRpc]
    private void SendInputToServer(float move, float turn)
    {
        // Server executes movement
        MoveBoat(move, turn);
    }

    [Server]
    private void MoveBoat(float move, float turn)
    {
        // Time.fixedDeltaTime used for consistent movement on server
        float moveAmount = move * moveSpeed * Time.fixedDeltaTime;
        float turnAmount = turn * turnSpeed * Time.fixedDeltaTime;

        transform.Translate(Vector3.forward * moveAmount);
        transform.Rotate(Vector3.up * turnAmount);
    }
}
