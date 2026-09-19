using FishNet.Object;
using UnityEngine;

public class Wheel : NetworkBehaviour
{
    [Header("Wheel Settings")]
    public float _currentAngle = 0f;
    [SerializeField] private Transform steeringWheel;

    private void Start()
    {
        if (steeringWheel == null)
            steeringWheel = transform.Find("SteeringWheel");

        if (steeringWheel == null)
        {
            Debug.LogError("Steering wheel visual reference is missing.", this);
        }
    }

    private void Update()
    {
        if (steeringWheel == null)
            return;

        steeringWheel.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
    }

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

    [ObserversRpc(ExcludeServer = true)]
    private void SyncAngleObserversRpc(float angle)
    {
        SetAngleLocal(angle);
    }

    private void SetAngleLocal(float angle)
    {
        _currentAngle = angle;
        ApplyVisualRotation(angle);
    }

    public float GetAngle()
    {
        return _currentAngle;
    }


    private void ApplyVisualRotation(float angle)
    {
        if (steeringWheel == null)
            return;

        steeringWheel.localRotation = Quaternion.Euler(0f, angle, 0f); // Adjust axis depending on model
    }
}
