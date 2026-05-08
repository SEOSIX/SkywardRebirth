using UnityEngine;

public class MusicsZones : MonoBehaviour
{
    [SerializeField] private Vector3 BoxSize;
    [SerializeField] private Color _colorBox;
    [SerializeField] private MusicTrack Track;
    [SerializeField] private float FadeDuration = 2f;

    void Start()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
            BoxSize = col.size;
        
        CheckPlayerInsideOnStart();
    }

    void CheckPlayerInsideOnStart()
    {
        GameObject player = Player.instance.gameObject;
        if (player == null) return;
        Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
        Vector3 halfSize = BoxSize / 2f;

        bool isInside = Mathf.Abs(localPos.x) <= halfSize.x &&
                        Mathf.Abs(localPos.y) <= halfSize.y &&
                        Mathf.Abs(localPos.z) <= halfSize.z;

        if (isInside)
        {
            if (MusicZoneManager.Instance == null)
            {
                return;
            }
            MusicZoneManager.Instance.TransitionTo(Track, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (MusicZoneManager.Instance == null)
        {
            return;
        }
        MusicZoneManager.Instance.TransitionTo(Track, FadeDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color  = _colorBox;
        Gizmos.DrawWireCube(Vector3.zero, BoxSize);
        _colorBox.a   = 0.5f;
        Gizmos.color  = _colorBox;
        Gizmos.DrawCube(Vector3.zero, BoxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    void Reset()
    {
        BoxCollider col = gameObject.GetComponent<BoxCollider>()
                       ?? gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = BoxSize == Vector3.zero ? Vector3.one : BoxSize;
    }
}