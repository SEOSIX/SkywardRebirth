using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovements : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;
    
    [Header("Snap")]
    public float snapSpeed = 8f;
    private bool _isSnapping = false;
    
    [Header("Distance")]
    public float distance = 5f;
    public float height = 2f;
    
    [Header("Smooth & Align")]
    [Range(1f, 20f)]
    public float positionSmooth = 5f;
    [Range(0.1f, 5f)]
    public float rotationSmooth = 1f;
    [Range(0f, 180f)]
    public float maxAlignAngle = 110f;

    [Header("Free Cam")]
    public float lookSensitivity = 3f;
    [Range(-80f, 0f)]
    public float minPitch = -30f;
    [Range(0f, 80f)]
    public float maxPitch = 60f;

    [Header("Collisions")]
    public LayerMask collisionLayers;
    public float cameraRadius = 0.3f;
    public float minDistance = 0.5f;

    private bool _isFreeCam = false;
    private float _currentPitch = 0f;
    private Vector2 _lookInput;

    private Vector3 _currentVelocity;
    private float _currentYaw;

    void Start()
    {
        _currentYaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetYaw = target.eulerAngles.y;

        if (_isFreeCam)
        {
            _currentYaw   += _lookInput.x * lookSensitivity;
            _currentPitch -= _lookInput.y * lookSensitivity;
            _currentPitch  = Mathf.Clamp(_currentPitch, minPitch, maxPitch);
        }
        else
        {
            _currentPitch = Mathf.Lerp(_currentPitch, 0f, rotationSmooth * Time.deltaTime);

            if (_isSnapping)
            {
                _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, snapSpeed * Time.deltaTime);
                if (Mathf.Abs(Mathf.DeltaAngle(_currentYaw, targetYaw)) < 0.5f)
                {
                    _currentYaw = targetYaw;
                    _isSnapping = false;
                }
            }
            else if (Player.instance.playerContoller.IsMoving)
            {
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(_currentYaw, targetYaw));

                if (angleDifference <= maxAlignAngle)
                {
                    _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, rotationSmooth * Time.deltaTime);
                }
            }
        }
        Quaternion camRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 offset = camRotation * new Vector3(0f, height, -distance);
        Vector3 desiredPosition = target.position + offset;
        
        Vector3 lookAtPoint = target.position + Vector3.up * height * 0.5f;

        Vector3 directionToCamera = (desiredPosition - lookAtPoint).normalized;
        float desiredDistance = Vector3.Distance(lookAtPoint, desiredPosition);
        Vector3 finalPosition = desiredPosition;

        if (Physics.SphereCast(lookAtPoint, cameraRadius, directionToCamera, out RaycastHit hit, desiredDistance, collisionLayers))
        {
            float clampedDistance = Mathf.Clamp(hit.distance, minDistance, desiredDistance);
            finalPosition = lookAtPoint + directionToCamera * clampedDistance;
        }
        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPosition,
            ref _currentVelocity,
            1f / positionSmooth
        );

        transform.LookAt(lookAtPoint);
    }
    
    public void OnFreeCam(InputAction.CallbackContext context)
    {
        if (context.performed)
            _isFreeCam = true;
        else if (context.canceled)
            _isFreeCam = false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }
    
    public void SnapBehindPlayer()
    {
        _isSnapping = !_isSnapping;
    }
}