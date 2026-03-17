using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Normal Surface")]
    public float normalSpeed = 8f;
    public float normalAcceleration = 20f;
    public float normalDamping = 6f;

    [Header("Slippery Surface")]
    public float slipperySpeed = 10f;
    public float slipperyAcceleration = 5f;
    public float slipperyDamping = 0.2f;

    [Header("Grip Surface")]
    public float gripSpeed = 6f;
    public float gripAcceleration = 35f;
    public float gripDamping = 12f;

    public List<Color> colors = new List<Color>();
    public GameObject cannon;
    public GameObject bullet;

    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;

    public float mouseSensitivity = 70f;
    private float xRotation = 0f;

    public float jumpForce = 5f;
    private bool isGrounded;
    private Rigidbody rb;

    private float currentSpeed;
    private float currentAcceleration;
    private float currentDamping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;

        SetNormalSurface();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        Vector3 inputDir = new Vector3(x, 0f, z).normalized;
        Vector3 worldMove = transform.TransformDirection(inputDir);

        Vector3 targetVelocity = worldMove * currentSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;
        velocityChange.y = 0f;

        rb.AddForce(velocityChange * currentAcceleration, ForceMode.Acceleration);
        rb.linearDamping = currentDamping;
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        CheckGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Color myColor = colors[(int)OwnerClientId];
            BulletSpawningServerRpc(cannon.transform.position, cannon.transform.rotation, myColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        Debug.Log("Touched: " + other.name);

        if (other.CompareTag("Slippery"))
        {
            Debug.Log("Entered Slippery Zone");
            currentSpeed = slipperySpeed;
            currentAcceleration = slipperyAcceleration;
            currentDamping = slipperyDamping;
        }
        else if (other.CompareTag("Grip"))
        {
            Debug.Log("Entered Grip Zone");
            currentSpeed = gripSpeed;
            currentAcceleration = gripAcceleration;
            currentDamping = gripDamping;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Slippery") || other.CompareTag("Grip"))
        {
            Debug.Log("Returned to Normal Surface");
            SetNormalSurface();
        }
    }

    void SetNormalSurface()
    {
        currentSpeed = normalSpeed;
        currentAcceleration = normalAcceleration;
        currentDamping = normalDamping;
    }

    public override void OnNetworkSpawn()
    {
        Color tint = colors[(int)OwnerClientId];

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * tint);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", mat.GetColor("_Color") * tint);
            }
        }

        if (!IsOwner) return;
        audioListener.enabled = true;
        playerCamera.enabled = true;
    }

    [ServerRpc]
    private void BulletSpawningServerRpc(Vector3 position, Quaternion rotation, Color color)
    {
        BulletSpawningClientRpc(position, rotation, color);
    }

    [ClientRpc]
    private void BulletSpawningClientRpc(Vector3 position, Quaternion rotation, Color color)
    {
        GameObject newBullet = Instantiate(bullet, position, rotation);
        newBullet.GetComponent<Renderer>().material.color = color;
        newBullet.GetComponent<Rigidbody>().linearVelocity += Vector3.up * 2;
        newBullet.GetComponent<Rigidbody>().AddForce(newBullet.transform.forward * -1500);
    }
}