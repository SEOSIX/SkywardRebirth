using System;
using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    private bool _isGrounded;
    private bool _hasJumped = false;
    private float _playerHalfHeight;

    void Start()
    {
        _playerHalfHeight = Player.instance.meshRenderer.bounds.extents.y;
    }

    private void FixedUpdate()
    {
        _isGrounded = CheckGrounded();
        AutoJump();
    }

    private bool CheckGrounded()
    {
        var data = Player.instance._data.controllerData;
        float offset = 0.01f;

        Vector3 origin = transform.position - Vector3.up * _playerHalfHeight;
        float maxDistance = data.groundCheckDistance + offset;
        
        bool isGrounded = Physics.Raycast(origin, Vector3.down, maxDistance, data.groundLayer);

        if (!isGrounded)
            _hasJumped = false;

        return isGrounded;
    }

    private void AutoJump()
    {
        if (_hasJumped) return;
        if (!_isGrounded) return;

        var data = Player.instance._data.controllerData;

        Vector3 horizontalVelocity = new Vector3(
            Player.instance.rigidbody.linearVelocity.x, 0,
            Player.instance.rigidbody.linearVelocity.z
        );

        float speed = horizontalVelocity.magnitude;
        if (speed < data.minSpeedToJump) return;

        
        Vector3 moveDirection = horizontalVelocity.magnitude > 0.01f 
            ? horizontalVelocity.normalized 
            : transform.forward;
        Vector3 feetPosition = transform.position;
        Vector3 rayOrigin = feetPosition + moveDirection * data.edgeDetectDistance;

        bool noGroundInFront = !Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, data.maxFallHeight, data.groundLayer);
        if (noGroundInFront)
            Jump();
    }

    private void Jump()
    {
        _hasJumped = true;

        Vector3 vel = Player.instance.rigidbody.linearVelocity;
        vel.y = Player.instance._data.controllerData.jumpForce;
        Player.instance.rigidbody.linearVelocity = vel;
    }

    private void OnDrawGizmos()
    {
        if (Player.instance == null) return;

        var data = Player.instance._data.controllerData;
        Vector3 horizontalVelocity = new Vector3(
            Player.instance.rigidbody.linearVelocity.x, 0,
            Player.instance.rigidbody.linearVelocity.z
        );
        
        Vector3 moveDirection = horizontalVelocity.magnitude > 0.01f 
            ? horizontalVelocity.normalized 
            : transform.forward;

        Vector3 feetPosition = transform.position;
        Vector3 rayOrigin = feetPosition + moveDirection * data.edgeDetectDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(feetPosition, moveDirection * data.edgeDetectDistance);
        Gizmos.DrawRay(rayOrigin, Vector3.down * data.maxFallHeight);
    }
}