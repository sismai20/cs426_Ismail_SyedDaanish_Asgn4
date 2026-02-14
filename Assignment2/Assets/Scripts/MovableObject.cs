using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

public class MovableObject : NetworkBehaviour
{
    [Header("Timer")]
    [FormerlySerializedAs("timeLimit")]
    [SerializeField] private float timeLimitSeconds = 10f;
    [SerializeField] private string playerTag = "Player";

    [Header("Goal Behavior")]
    [SerializeField] private bool despawnOnGoal = true;
    [FormerlySerializedAs("despawnDelaySeconds")]
    [SerializeField] private float despawnDelay = 0.5f;

    private Rigidbody body;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float countdown;
    private bool timerRunning;
    private bool goalReached;

    private readonly NetworkVariable<float> syncedTimeLeft = new(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> syncedTimerRunning = new(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> syncedReachedGoal = new(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        ResetRoundState();
    }

    private void Update()
    {
        if (!IsServer || goalReached || !timerRunning) return;

        countdown -= Time.deltaTime;

        if (countdown <= 0f)
        {
            ResetToSpawn();
            return;
        }

        PushNetworkState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer || goalReached || timerRunning) return;

        if (collision.collider.CompareTag(playerTag))
        {
			Debug.Log("Entering push state");
            timerRunning = true;
            PushNetworkState();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || goalReached) return;

        if (other.CompareTag("Goal"))
        {
            goalReached = true;
            timerRunning = false;
            PushNetworkState();

            if (despawnOnGoal)
            {
                StartCoroutine(DespawnAfterDelay());
            }
        }
    }

    [ContextMenu("Reset Movable Object To Spawn")]
    public void ResetToSpawn()
    {
        if (!IsServer) return;

        ResetRoundState();

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = initialPosition;
            body.rotation = initialRotation;
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    public float GetTimeRemaining() => countdown;
    public float GetSyncedTimeRemaining() => syncedTimeLeft.Value;
    public bool IsTimerRunning() => syncedTimerRunning.Value;
    public bool HasReachedGoal() => syncedReachedGoal.Value;

    private IEnumerator DespawnAfterDelay()
    {
        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        if (despawnDelay > 0f)
        {
            yield return new WaitForSeconds(despawnDelay);
        }

        if (!IsServer) yield break;

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ResetRoundState()
    {
        timerRunning = false;
        goalReached = false;
        countdown = timeLimitSeconds;
        PushNetworkState();
    }

    private void PushNetworkState()
    {
        if (!IsServer) return;

        syncedTimeLeft.Value = Mathf.Max(0f, countdown);
        syncedTimerRunning.Value = timerRunning;
        syncedReachedGoal.Value = goalReached;
    }
}
