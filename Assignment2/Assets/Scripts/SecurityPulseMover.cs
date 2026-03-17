using UnityEngine;

public class SecurityPulseMover : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.right; // direction of motion
    public float distance = 6f;                   // how far it travels
    public float speed = 2f;                      // how fast it moves

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        // ping-pong between 0 and distance
        float t = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + moveDirection * t;
    }
}