using UnityEngine;

public class BirdZone : MonoBehaviour
{
    [SerializeField]private Vector3 BoxSize;


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, BoxSize);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawCube(transform.position, BoxSize);
    }
}
