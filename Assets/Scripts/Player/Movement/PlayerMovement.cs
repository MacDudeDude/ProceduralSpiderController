using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Transform bodyForward;
    [SerializeField] private Transform bodyHead;
    [SerializeField] private LayerMask collisionLayers;

    private SpiderState state;
    private PlayerOrientation bodyHandler;
    private Rigidbody rb;
    private Vector3 input;

    private Vector3 startingPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyHandler = GetComponent<PlayerOrientation>();
        state = transform.root.GetComponent<SpiderState>();
        startingPosition = rb.position;
    }

    private void Update()
    {
        GetInputs();
        CheckResetPosition();
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
    }

    private void Move()
    {
        Vector3 newPosition = bodyHandler.GetNewHeightPosition();
        Vector3 movingPlatformOffset = bodyHandler.GetPlatformOffset();
        Vector3 forces = GetMovementForce();

        if (Physics.Linecast(rb.position, newPosition + movingPlatformOffset + forces * Time.fixedDeltaTime, collisionLayers))
            return;

        rb.MovePosition(newPosition + movingPlatformOffset + forces * Time.fixedDeltaTime);
    }

    private Vector3 GetMovementForce()
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Falling:
            case SpiderState.MovementState.Jumping:
                return bodyHead.rotation * input * speed;
            case SpiderState.MovementState.Descending:
                return Vector3.zero;
        }

        return bodyForward.rotation * input * speed;
    }

    private void CheckResetPosition()
    {
        if (rb.position.y < -70)
            transform.position = startingPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.GetChild(0).rotation * input * 5);
    }
}
