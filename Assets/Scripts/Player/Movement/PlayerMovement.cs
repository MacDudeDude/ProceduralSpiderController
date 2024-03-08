using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform bodyForward;
    [SerializeField] private LayerMask collisionLayers;

    private SpiderState state;
    private PlayerOrientation bodyHandler;
    private Rigidbody rb;
    private Vector3 input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyHandler = GetComponent<PlayerOrientation>();
        state = transform.root.GetComponent<SpiderState>();
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GetInputs()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.z = Input.GetAxisRaw("Vertical");
        input.Normalize();

        if (state.currentState == SpiderState.MovementState.Descending)
            input = Vector3.zero;

        if (state.lockInputForces)
            input = Vector3.zero;
    }

    private void Move()
    {
        Vector3 newPosition = bodyHandler.GetNewHeightPosition();
        Vector3 forces = bodyForward.rotation * input * speed;

        if (Physics.Linecast(rb.position, newPosition + forces * Time.fixedDeltaTime, collisionLayers))
            return;

        rb.MovePosition(newPosition + forces * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.GetChild(0).rotation * input * 5);
    }
}
