using UnityEngine;

namespace Entiti
{
    public class PlayerPhysic : MonoBehaviour
    {
        [Header("Snap au Sol")]
        [Tooltip("La force appliquée pour plaquer le joueur au sol.")]
        [SerializeField] private float snapForce = 50f;
        [Tooltip("La distance du rayon vers le bas (à ajuster selon la taille du collider).")]
        [SerializeField] private float groundCheckDistance = 1.1f; 

        [Header("Gestion des Pentes")]
        [Tooltip("L'angle maximum que le joueur peut grimper.")]
        [SerializeField] private float maxSlopeAngle = 45f;
        [Tooltip("La distance de détection devant le joueur.")]
        [SerializeField] private float forwardCheckDistance = 0.5f;
        
        [Header("Calques (Layers)")]
        [SerializeField] private LayerMask collisionLayer;

        private void FixedUpdate()
        {
            if (Player.instance == null || Player.instance.rigidbody == null) return;

            Rigidbody rb = Player.instance.rigidbody;
            HandleGravityAndSlopes(rb);
        }

        private void HandleGravityAndSlopes(Rigidbody rb)
        {
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, groundCheckDistance, collisionLayer);

            Vector3 direction = rb.linearVelocity.sqrMagnitude > 0.1f ? rb.linearVelocity.normalized : transform.forward;
            direction.y = 0;

            bool isFacingObstacle = Physics.Raycast(transform.position, direction, out RaycastHit forwardHit, forwardCheckDistance, collisionLayer);

            bool isFacingClimbableSlope = false;
            bool isFacingSteepSlope = false;

            if (isFacingObstacle)
            {
                float slopeAngle = Vector3.Angle(Vector3.up, forwardHit.normal);

                if (slopeAngle > 0f && slopeAngle < 89f)
                {
                    if (slopeAngle <= maxSlopeAngle)
                    {
                        isFacingClimbableSlope = true;
                    }
                    else
                    {
                        isFacingSteepSlope = true;
                    }
                }
            }
            if (isGrounded)
            {
                if (isFacingClimbableSlope)
                {
                    
                }
                else
                {
                    rb.AddForce(Vector3.down * snapForce, ForceMode.Acceleration);
                }
            }

            if (isFacingSteepSlope && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * forwardCheckDistance);
        }
    }
}