using UnityEngine;

namespace Entiti
{
    public class PlayerPhysic : MonoBehaviour
    {
        [Header("Snap au Sol")]
        [SerializeField] private float snapForce = 50f;
        [Tooltip("La distance du rayon vers le bas (à ajuster selon la taille du collider).")]
        [SerializeField] private float groundCheckDistance = 1.1f; 

        [Header("Gestion des Pentes")]
        [Tooltip("L'angle maximum que le joueur peut grimper.")]
        [SerializeField] private float maxSlopeAngle = 45f;
        [Tooltip("La distance de détection devant le joueur.")]
        [SerializeField] private float forwardCheckDistance = 0.5f;

        [Header("Gestion des Marches")]
        [SerializeField] private float stepCheckDistance = 0.5f;
        [SerializeField] private float maxStepHeight = 0.5f;
        [SerializeField] private float smoothingTp = 5f;
        
        [Header("Calques (Layers)")]
        [SerializeField] private LayerMask collisionLayer;

        private bool isLerping = false;
        private Vector3 targetPosition;
        
        private void FixedUpdate()
        {
            if (Player.instance == null || Player.instance.rigidbody == null) return;
            Rigidbody rb = Player.instance.rigidbody;

            // --- CORRECTION 1 : Si on grimpe, on gère UNIQUEMENT le Lerp et on stoppe le reste ---
            if (isLerping)
            {
                // On déplace le Rigidbody de manière fluide (mieux que transform.position)
                rb.position = Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * smoothingTp);
            
                // Fin du déplacement
                if (Vector3.Distance(rb.position, targetPosition) < 0.05f)
                {
                    rb.position = targetPosition; // Snap final parfait
                    rb.isKinematic = false;       // On réactive la physique !
                    isLerping = false;
                }
                return; // IMPORTANT : On quitte la fonction ici pour ne pas appliquer la gravité/pentes
            }

            // Physique normale (seulement si on ne grimpe pas)
            HandleGravityAndSlopes(rb);
            
            // On ne cherche une marche que si le joueur avance
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                HandleStep(rb);
            }
        }

        private void HandleGravityAndSlopes(Rigidbody rb)
        {
            bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, groundCheckDistance, collisionLayer);
            Vector3 direction = rb.linearVelocity.sqrMagnitude > 0.1f ? rb.linearVelocity : transform.forward;
            direction.y = 0;
            direction = direction.normalized;

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
                if (!isFacingClimbableSlope) 
                {
                    rb.AddForce(Vector3.down * snapForce, ForceMode.Acceleration);
                }
            }
            
            if (isFacingSteepSlope && rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
        }

        void HandleStep(Rigidbody rb)
        {
            Vector3 lowerOrigin = transform.position + transform.up * 0.05f;
            RaycastHit hitLower;
            bool lowerHit = Physics.Raycast(lowerOrigin, transform.forward, out hitLower, stepCheckDistance, collisionLayer);
        
            Vector3 upperOrigin = transform.position + transform.up * maxStepHeight;
            RaycastHit hitUpper;
            bool upperHit = Physics.Raycast(upperOrigin, transform.forward, out hitUpper, stepCheckDistance, collisionLayer);
            
            if (lowerHit && !upperHit)
            {
                // --- CORRECTION 2 : Placer l'origine du rayon du haut BIEN AU-DESSUS de la marche ---
                // On prend la position XZ de l'impact, on avance un poil, et on force la hauteur Y au-dessus de la hauteur max
                Vector3 downRayOrigin = hitLower.point + (transform.forward * 0.1f);
                downRayOrigin.y = transform.position.y + maxStepHeight + 0.1f; 

                RaycastHit hitDownward;

                // On tire vers le bas
                if (Physics.Raycast(downRayOrigin, Vector3.down, out hitDownward, maxStepHeight + 0.2f, collisionLayer))
                {
                    targetPosition = hitDownward.point; 
                    
                    // --- CORRECTION 3 : On coupe la physique pour éviter les conflits pendant le Lerp ---
                    rb.isKinematic = true; 
                    isLerping = true; 
                }
            }
        }
    }
}