using UnityEngine;

public class SecurityPulseKnockback : MonoBehaviour
{
    public float knockbackForce = 12f;
    public float upwardForce = 1.5f; // small lift so it feels like impact

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        Rigidbody playerRb = collision.collider.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        // Direction from pulse -> player
        Vector3 dir = (collision.collider.transform.position - transform.position).normalized;
        dir.y = 0f;

        Vector3 force = dir * knockbackForce + Vector3.up * upwardForce;
        playerRb.AddForce(force, ForceMode.Impulse);
    }
}