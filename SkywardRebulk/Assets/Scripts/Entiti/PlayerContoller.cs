using Scriptable;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerContoller : MonoBehaviour
{
    #region Variables
    
    public Camera _cam;
    public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;
    private Vector2 _moveInput;
    private Vector3 _lastDirection;

    #endregion
    

    private void FixedUpdate()
    {
        Move();
    }

    #region Inputs

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            _moveInput = context.ReadValue<Vector2>();
        else if (context.canceled)
            _moveInput = Vector2.zero;
    }
    
    public void OnFreeCam(InputAction.CallbackContext context)
    {
        Player.instance.cameraMovements.OnFreeCam(context);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Player.instance.cameraMovements.OnLook(context);
    }

    public void OnTarget(InputAction.CallbackContext context)
    {
        if (context.performed)
            Player.instance.cameraMovements.SnapBehindPlayer();
        else if (context.canceled)
            Player.instance.cameraMovements.SnapBehindPlayer();
    }

    #endregion
    

    #region MOVEMENTS

    
    private void Move()
    {
        Vector2 input = Vector2.ClampMagnitude(_moveInput, 1f);

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = (camForward * input.y + camRight * input.x);
        if (direction.sqrMagnitude > 0.01f)
            _lastDirection = direction;

        Vector3 targetVelocity = direction * Player.instance._data.controllerData.walkSpeed;
        targetVelocity.y = Player.instance.rigidbody.linearVelocity.y;
        Vector3 velocityDiff = targetVelocity - Player.instance.rigidbody.linearVelocity;
        velocityDiff.y = 0f;
        Player.instance.rigidbody.AddForce(velocityDiff, ForceMode.VelocityChange);

        if (_lastDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.Euler(
                0f,
                Quaternion.LookRotation(_lastDirection).eulerAngles.y,
                0f
            );

            Player.instance.rigidbody.MoveRotation(Quaternion.Slerp(
                Player.instance.rigidbody.rotation,
                targetRotation,
                Player.instance._data.controllerData.rotationSpeed * Time.fixedDeltaTime));
        }
    }

    #endregion
}