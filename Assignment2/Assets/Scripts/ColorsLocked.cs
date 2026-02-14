using UnityEngine;
using Unity.Netcode;

public class ColorsLocked : NetworkBehaviour
{
    public Color allowedColor;
    private Rigidbody rb;
    private bool ownershipClaimed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

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
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player != null && ColorsMatch(player.colors[(int)player.OwnerClientId], allowedColor))
        {
            // claim permanent ownership on first contact
            if (!ownershipClaimed)
            {
                ownershipClaimed = true;
                ClaimOwnershipRpc(player.OwnerClientId);
            }
            return;
        }

        // everything else — freeze
        rb.isKinematic = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerMovement>();
        if (player != null)
        {
            rb.isKinematic = false;
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	private void ClaimOwnershipRpc(ulong clientId)
	{
		GetComponent<NetworkObject>().ChangeOwnership(clientId);
	}

    private bool ColorsMatch(Color a, Color b)
    {
        return Vector4.Distance((Vector4)a, (Vector4)b) < 0.3f;
    }
}