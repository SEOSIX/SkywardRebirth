using UnityEngine;

public class CameraMovements : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 2f, -5f);

    [Header("Smooth")]
    [Range(1f, 20f)]
    public float smoothSpeed = 5f;

    private Vector3 _currentVelocity; 

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            1f / smoothSpeed 
        );
        transform.LookAt(target.position);
    }
}
