using FishNet.Object;
using UnityEngine;

public class Wheel : NetworkBehaviour
{
    [Header("Wheel Settings")]
    public float _currentAngle = 0f;
    [SerializeField] private Transform steeringWheel;

    // Finds the visual steering wheel child if it was not assigned in the Inspector.
    private void Start()
    {
        if (steeringWheel == null)
            steeringWheel = transform.Find("SteeringWheel");

        if (steeringWheel == null)
        {
            Debug.LogError("Steering wheel visual reference is missing.", this);
        }
    }

    // Keeps the wheel mesh matched to the current steering angle.
    private void Update()
    {
        if (steeringWheel == null)
            return;

        steeringWheel.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
    }

    // Sets an absolute wheel angle and syncs that visual state to observers when running on the server.
    public void SetAngle(float angle)
    {
        if (IsServerInitialized)
        {
            SetAngleLocal(angle);
            SyncAngleObserversRpc(_currentAngle);
            return;
        }

        SetAngleLocal(angle);
    }

    // Adds steering input to the current wheel angle and syncs it in networked play.
    public void AddAngle(float angle)
    {
        if (IsServerInitialized)
        {
            SetAngleLocal(_currentAngle + angle);
            SyncAngleObserversRpc(_currentAngle);
            return;
        }

        SetAngleLocal(_currentAngle + angle);
    }

    // FishNet sends server wheel angle changes to non-server clients.
    [ObserversRpc(ExcludeServer = true)]
    private void SyncAngleObserversRpc(float angle)
    {
        SetAngleLocal(angle);
    }

    // Stores the angle and applies the visual rotation in one place.
    private void SetAngleLocal(float angle)
    {
        _currentAngle = angle;
        ApplyVisualRotation(angle);
    }

    public float GetAngle()
    {
        return _currentAngle;
    }


    // Rotates the steering wheel mesh without changing gameplay state.
    private void ApplyVisualRotation(float angle)
    {
        if (steeringWheel == null)
            return;

        steeringWheel.localRotation = Quaternion.Euler(0f, angle, 0f); // Adjust axis depending on model
    }
}
