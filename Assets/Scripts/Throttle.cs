using FishNet.Object;
using UnityEngine;

public class Throttle : NetworkBehaviour
{
    [Header("Throttle Settings")]
    private float _throttleValue = 0f;       // Current lever position
    public float _throttleApplied = 0f;     // Target lever position
    public float _maxThrottle = 0.10f;

    public float leverMoveSpeed = 1f;            // modifyer for the lever movement animation

    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.localPosition; // Store the lever's rest position
    }

    void Update()
    {
        // do nothing if the throttleValue hasnt changed (significantly)
        if (Mathf.Approximately(_throttleValue, _throttleApplied))
            return;

        // Smoothly move the lever toward the desired position
        _throttleValue = Mathf.MoveTowards(_throttleValue, _throttleApplied, leverMoveSpeed * Time.deltaTime);

        // Convert throttleValue to a local offset from base position
        transform.localPosition = basePosition + (transform.forward * _throttleValue);

        // clamping the throttle value to the maximum throttle
        _throttleApplied = Mathf.Clamp(_throttleApplied, -_maxThrottle, _maxThrottle);

    }
}
