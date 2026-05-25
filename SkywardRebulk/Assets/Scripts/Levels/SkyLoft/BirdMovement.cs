using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float minSpeed = 1.5f;
    [SerializeField] private float maxSpeed = 3.5f;

    [Header("Direction change")]
    [SerializeField] private float minTimeBetweenTurns = 1.5f;
    [SerializeField] private float maxTimeBetweenTurns = 4f;
    [SerializeField] private float turnSharpness = 3f;
    [SerializeField] private float linearBias = 0.6f;

    [Header("Bank on turn")]
    [SerializeField] private float maxBankAngle = 25f;
    [SerializeField] private float bankSpeed = 4f;

    private BirdZone zone;
    private Vector3 currentDirection;
    private Vector3 targetDirection;
    private float speed;
    private float turnTimer;
    private float targetBankY;
    private float currentBankY;

    public void Initialize(BirdZone birdZone)
    {
        zone = birdZone;
    }

    void Start()
    {
        if (zone == null)
            zone = GetComponentInParent<BirdZone>();

        speed = Random.Range(minSpeed, maxSpeed);
        currentDirection = Random.onUnitSphere;
        currentDirection.y *= 0.3f;
        currentDirection.Normalize();
        targetDirection = currentDirection;

        turnTimer = Random.Range(0f, maxTimeBetweenTurns);
    }

    void Update()
    {
        HandleTurnTimer();
        MoveAndSteer();
        ApplyBankRotation();
        StayInBounds();
    }

    void HandleTurnTimer()
    {
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0f)
        {
            PickNewDirection();
            turnTimer = Random.Range(minTimeBetweenTurns, maxTimeBetweenTurns);
        }
    }

    void PickNewDirection()
    {
        Vector3 randomDir = Random.onUnitSphere;
        randomDir.y *= 0.3f;
        randomDir.Normalize();

        targetDirection = Vector3.Lerp(randomDir, currentDirection, linearBias).normalized;
    }

    void MoveAndSteer()
    {
        currentDirection = Vector3.Lerp(currentDirection, targetDirection, turnSharpness * Time.deltaTime).normalized;

        transform.position += currentDirection * speed * Time.deltaTime;

        if (currentDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSharpness * Time.deltaTime);
        }
        float turnDot = Vector3.Cross(currentDirection, targetDirection).y;
        targetBankY = -turnDot * maxBankAngle;
    }

    void ApplyBankRotation()
    {
        currentBankY = Mathf.Lerp(currentBankY, targetBankY, bankSpeed * Time.deltaTime);

        transform.rotation *= Quaternion.Euler(0f, 0f, currentBankY);
    }

    void StayInBounds()
    {
        if (!zone.IsInsideBox(transform.position))
        {
            Vector3 toCenter = (zone.transform.position - transform.position).normalized;
            targetDirection = toCenter;
        }
    }
}