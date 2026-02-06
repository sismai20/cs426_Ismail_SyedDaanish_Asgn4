using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float movementSpeed = 8.0f;
    public float rotationSpeed = 90.0f;
    public float gravity = 20.0f;
    public float jumpForce = 5.0f;

    public GameObject cannon;
    public GameObject bullet;
    public float bulletForwardForce = 1500f;

    [SerializeField] private GameObject spawnedPrefab;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera playerCamera;
    
    private GameObject instantiatedPrefab;
    private Rigidbody rb;
    private Vector2 moveInput;
    private float rotationInput;
    private bool jumpRequested;

    /// <summary>
    /// sets up audio, camera, rotation locks, rigidbody/physics for
    /// the player (interpolation of differing movements disabled)
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        // Ensure constraints are set so physics doesn't tip the player over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (audioListener != null) audioListener.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;
    }

    /// <summary>
    /// update every frame the movement, rotation,
    /// shooting, and object spawn from the cannon
    /// </summary>
    private void Update()
    {
        if (!IsOwner) return;

        HandleInput();
        HandleRotation();
        HandleCombat();
        HandleObjectSpawning();
    }

    /// <summary>
    /// Application of the movement is set at 50fps for stability,
    /// doesn't change from it for network stability
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsOwner) return;

        ApplyMovement();
    }

    /// <summary>
    /// Handles movement input including the jump
    /// </summary>
    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        rotationInput = 0;
        if (keyboard.dKey.isPressed) rotationInput = 1;
        else if (keyboard.aKey.isPressed) rotationInput = -1;

        moveInput.y = 0;
        if (keyboard.wKey.isPressed) moveInput.y = 1;
        else if (keyboard.sKey.isPressed) moveInput.y = -1;

        if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    /// <summary>
    /// handles rotation from the a/d keys
    /// </summary>
    private void HandleRotation()
    {
        float rotation = rotationInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }

    /// <summary>
    /// Applies the movement of the player including jumping
    /// </summary>
    private void ApplyMovement()
    {
        Vector3 move = transform.forward * moveInput.y * movementSpeed;
        
        Vector3 velocity = rb.linearVelocity;
        
        velocity.x = move.x;
        velocity.z = move.z;

        if (jumpRequested)
        {
            velocity.y = jumpForce;
            jumpRequested = false;
        }

        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Shooting handler
    /// </summary>
    private void HandleCombat()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            BulletSpawningServerRpc(cannon.transform.position, cannon.transform.rotation);
        }
    }

    /// <summary>
    /// Object creation from the shoot button with i and j to spawn objects
    /// while testing
    /// 
    /// USE THIS SPECIFICALLY FOR TESTING
    /// </summary>
    private void HandleObjectSpawning()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.iKey.wasPressedThisFrame)
        {
            SpawnObjectServerRpc();
        }

        if (keyboard.jKey.wasPressedThisFrame)
        {
            DespawnObjectServerRpc();
        }
    }

    /// <summary>
    /// quick check to see if we're grounded
    /// </summary>
    /// <returns> true if grounded, false if not</returns>
    private bool IsGrounded()
    {
        // Simple raycast check for Rigidbody-based jumping
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    /// <summary>
    /// creates object on server
    /// </summary>
    [ServerRpc]
    private void SpawnObjectServerRpc()
    {
        instantiatedPrefab = Instantiate(spawnedPrefab);
        instantiatedPrefab.GetComponent<NetworkObject>().Spawn(true);
    }

    /// <summary>
    /// Despawns an object on the server
    /// </summary>
    [ServerRpc]
    private void DespawnObjectServerRpc()
    {
        if (instantiatedPrefab != null)
        {
            instantiatedPrefab.GetComponent<NetworkObject>().Despawn(true);
            Destroy(instantiatedPrefab);
        }
    }

    /// <summary>
    ///  creates the bullet on the  server side
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    [ServerRpc]
    private void BulletSpawningServerRpc(Vector3 position, Quaternion rotation)
    {
        BulletSpawningClientRpc(position, rotation);
    }

    /// <summary>
    /// Spawns the bullet on the clientside
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    [ClientRpc]
    private void BulletSpawningClientRpc(Vector3 position, Quaternion rotation)
    {
        GameObject newBullet = Instantiate(bullet, position, rotation);
        Rigidbody bulletRb = newBullet.GetComponent<Rigidbody>();
        
        if (bulletRb != null)
        {
            bulletRb.linearVelocity += Vector3.up * 2;
            bulletRb.AddForce(newBullet.transform.forward * bulletForwardForce);
        }
    }
}