using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// adding namespaces
using Unity.Netcode;
// because we are using the NetworkBehaviour class
// NewtorkBehaviour class is a part of the Unity.Netcode namespace
// extension of MonoBehaviour that has functions related to multiplayer
public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    // create a list of colors
    public List<Color> colors = new List<Color>();
    public GameObject cannon;
    public GameObject bullet;
    // reference to the camera audio listener
    [SerializeField] private AudioListener audioListener;
    // reference to the camera
    [SerializeField] private Camera playerCamera;
	public float mouseSensitivity = 70f;
	private float xRotation = 0f;
	
	public float jumpForce = 5f;
    private bool isGrounded;
    private Rigidbody rb;
    
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		Cursor.lockState = CursorLockMode.Locked;
	}
	void FixedUpdate()
	{
		if (!IsOwner) return;
		Vector3 moveDirection = Vector3.zero;
		if (Input.GetKey(KeyCode.W)) moveDirection.z = +1f;
		if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
		if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
		if (Input.GetKey(KeyCode.D)) moveDirection.x = +1f;
		// transform direction from local to world space
		Vector3 worldMove = transform.TransformDirection(moveDirection.normalized);
		Vector3 velocity = worldMove * speed;
		rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
	}
	
	void CheckGrounded()
	{
		// cast a short ray downward from the player's position
		isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
	}
	
	void Update()
	{
		if (!IsOwner) return;
		// mouse look
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
		transform.Rotate(Vector3.up * mouseX);
		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
		// jump
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
    
    public override void OnNetworkSpawn()
	{
		Color tint = colors[(int)OwnerClientId];
		
		// get all renderers on this object and its children
		foreach (var renderer in GetComponentsInChildren<Renderer>())
		{
			foreach (var mat in renderer.materials)
			{
				// works for URP/HDRP Lit shaders
				if (mat.HasProperty("_BaseColor"))
					mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * tint);
				// works for Built-in Standard shader
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