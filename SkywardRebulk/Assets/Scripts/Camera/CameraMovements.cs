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
    [Header("Smooth")]
    [Range(1f, 20f)]
    public float positionSmooth = 5f;
    [Range(0.1f, 5f)]
    public float rotationSmooth = 1f;
    [Header("Free Cam")]
    public float lookSensitivity = 3f;
    [Range(-80f, 0f)]
    public float minPitch = -30f;
    [Range(0f, 80f)]
    public float maxPitch = 60f;

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
                _currentYaw = Mathf.LerpAngle(_currentYaw, targetYaw, rotationSmooth * Time.deltaTime);
            }
        }

        Quaternion camRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        Vector3 offset = camRotation * new Vector3(0f, height, -distance);
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _currentVelocity,
            1f / positionSmooth
        );

        transform.LookAt(target.position + Vector3.up * height * 0.5f);
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