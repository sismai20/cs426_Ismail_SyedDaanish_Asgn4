using UnityEngine;
using Unity.Netcode;

public class Target : NetworkBehaviour
{
    [Header("Timer")]
    [SerializeField] private float timeLimit = 10f;
    [SerializeField] private float moveStartDistance = 0.2f;
    [SerializeField] private float minMoveSpeed = 0.1f;

    private Rigidbody rb;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float timeRemaining;
    private bool timerStarted;
    private bool reachedGoal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        timeRemaining = timeLimit;
        timerStarted = false;
        reachedGoal = false;
    }

    private void Update()
    {
        if (!IsServer || reachedGoal) return;

        if (!timerStarted && HasStartedMoving())
        {
            timerStarted = true;
        }

        if (!timerStarted) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            ResetToSpawn();
        }
    }

    private bool HasStartedMoving()
    {
        float distFromSpawn = Vector3.Distance(transform.position, spawnPosition);
        bool movedEnough = distFromSpawn >= moveStartDistance;
        bool hasSpeed = rb != null && rb.linearVelocity.magnitude >= minMoveSpeed;
        return movedEnough || hasSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || reachedGoal) return;

        if (other.CompareTag("Goal"))
        {
            reachedGoal = true;
            timerStarted = false;
        }
    }

    [ContextMenu("Reset Target To Spawn")]
    public void ResetToSpawn()
    {
        if (!IsServer) return;

        timerStarted = false;
        reachedGoal = false;
        timeRemaining = timeLimit;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPosition;
            rb.rotation = spawnRotation;
        }
        else
        {
            transform.position = spawnPosition;
            transform.rotation = spawnRotation;
        }
    }

    public float GetTimeRemaining() => timeRemaining;
}
