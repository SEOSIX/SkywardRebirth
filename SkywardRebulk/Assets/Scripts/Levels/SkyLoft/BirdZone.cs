using UnityEngine;

public class BirdZone : MonoBehaviour
{
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private int birdCount = 4;
    [SerializeField] private GameObject birdPrefab;

    void Start()
    {
        SpawnMissingBirds();
    }

    void SpawnMissingBirds()
    {
        int missing = birdCount - transform.childCount;

        for (int i = 0; i < missing; i++)
        {
            Vector3 spawnPos = GetRandomPositionInBox();

            int attempts = 0;
            while (attempts < 10 && IsTooCloseToSibling(spawnPos, 1.5f))
            {
                spawnPos = GetRandomPositionInBox();
                attempts++;
            }

            GameObject bird = Instantiate(birdPrefab, spawnPos, Random.rotation);
            bird.transform.SetParent(transform);

            if (bird.TryGetComponent(out BirdMovement bm))
            {
                bm.Initialize(this);
            }
        }
    }

    bool IsTooCloseToSibling(Vector3 pos, float minDist)
    {
        foreach (Transform child in transform)
        {
            if (Vector3.Distance(child.position, pos) < minDist)
                return true;
        }
        return false;
    }

    public Vector3 GetRandomPositionInBox()
    {
        return new Vector3(
            transform.position.x + Random.Range(-boxSize.x / 2f, boxSize.x / 2f),
            transform.position.y + Random.Range(-boxSize.y / 2f, boxSize.y / 2f),
            transform.position.z + Random.Range(-boxSize.z / 2f, boxSize.z / 2f)
        );
    }

    public bool IsInsideBox(Vector3 pos)
    {
        Vector3 local = pos - transform.position;
        return Mathf.Abs(local.x) < boxSize.x / 2f &&
               Mathf.Abs(local.y) < boxSize.y / 2f &&
               Mathf.Abs(local.z) < boxSize.z / 2f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawCube(transform.position, boxSize);
    }
}