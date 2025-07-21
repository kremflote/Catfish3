using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Wheel : NetworkBehaviour
{
    [Header("Wheel Settings")]
    public float _currentAngle = 0f;
    private Transform steeringWheel;

    private void Start()
    {
        steeringWheel = transform.Find("SteeringWheel");
        if (steeringWheel == null)
        {
            Debug.LogError("SteeringWheel child not found under " + gameObject.name);
        }
    }

    private void Update()
    {
        steeringWheel.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
    }

    public void SetAngle(float angle)
    {
        _currentAngle = angle;
        ApplyVisualRotation(angle);
    }

    public void AddAngle(float angle)
    {
        _currentAngle += angle;
        ApplyVisualRotation(_currentAngle);
    }

    public float GetAngle()
    {
        return _currentAngle;
    }


    private void ApplyVisualRotation(float angle)
    {
        steeringWheel.localRotation = Quaternion.Euler(0f, angle, 0f); // Adjust axis depending on model
    }
}
