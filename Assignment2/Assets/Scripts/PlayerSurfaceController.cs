using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerSurfaceController : MonoBehaviour
{
    public float normalMoveForce = 12f;
    public float slipperyMoveForce = 5f;
    public float gripMoveForce = 18f;

    public float normalDrag = 2f;
    public float slipperyDrag = 0.2f;
    public float gripDrag = 6f;

    private Rigidbody rb;
    private float currentMoveForce;
    private float currentDrag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetNormalSurface();
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v);
        rb.AddForce(move * currentMoveForce, ForceMode.Force);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slippery"))
        {
            currentMoveForce = slipperyMoveForce;
            currentDrag = slipperyDrag;
            rb.linearDamping = currentDrag;
        }
        else if (other.CompareTag("Grip"))
        {
            currentMoveForce = gripMoveForce;
            currentDrag = gripDrag;
            rb.linearDamping = currentDrag;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Slippery") || other.CompareTag("Grip"))
        {
            SetNormalSurface();
        }
    }

    void SetNormalSurface()
    {
        currentMoveForce = normalMoveForce;
        currentDrag = normalDrag;
        rb.linearDamping = currentDrag;
    }
}