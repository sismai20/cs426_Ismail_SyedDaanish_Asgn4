using UnityEngine;

public class ColorsLocked : MonoBehaviour
{
    public Color allowedColor;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // still prevent flipping

        // tint all renderers on this box
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * allowedColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", mat.GetColor("_Color") * allowedColor);
            }
        }
    }

    void FixedUpdate()
    {
        // allow gravity (falling) but prevent bouncing/launching upward
        if (rb.linearVelocity.y > 0)
        {
        //    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player == null) return;

        Color playerColor = player.colors[(int)player.OwnerClientId];

        if (!ColorsMatch(playerColor, allowedColor))
        {
            rb.isKinematic = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
        {
            rb.isKinematic = false;
        }
    }

    private bool ColorsMatch(Color a, Color b)
    {
        return Vector4.Distance((Vector4)a, (Vector4)b) < 0.3f;
    }
}