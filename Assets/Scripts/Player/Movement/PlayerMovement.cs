using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float inputSmoothing;
    [SerializeField] private Transform bodyForward;
    [SerializeField] private Transform bodyHead;
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private GameObject graphics;
    [SerializeField] private GameObject graphicsRenderer;

    private SpiderState state;
    private PlayerOrientation bodyHandler;
    private Rigidbody rb;
    public Vector3 input;
    private Vector3 inputUnsmoothed;

    private Vector3 startingPosition;
    bool isDead;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyHandler = GetComponent<PlayerOrientation>();
        state = transform.root.GetComponent<SpiderState>();
        startingPosition = rb.position;
    }

    private void Update()
    {
        if (isDead)
            return;

        GetInputs();
        CheckResetPosition();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        Move();
    }

    private void GetInputs()
    {
        inputUnsmoothed.x = Input.GetAxisRaw("Horizontal");
        inputUnsmoothed.z = Input.GetAxisRaw("Vertical");
        inputUnsmoothed.Normalize();

        input.x = Mathf.Lerp(input.x, inputUnsmoothed.x, inputSmoothing * Time.deltaTime);
        input.z = Mathf.Lerp(input.z, inputUnsmoothed.z, inputSmoothing * Time.deltaTime);
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
        if (state.lockInputForces)
            return Vector3.zero;

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
            ResetPosition();
    }

    public void ResetPosition()
    {
        if (isDead)
            return;

        isDead = true;
        bodyHandler.LoseReferences();
        state.currentState = SpiderState.MovementState.Default;
        bodyHandler.enabled = false;
        graphicsRenderer.gameObject.SetActive(false);
        graphics.gameObject.SetActive(false);
        rb.position = startingPosition;
        Invoke(nameof(Enables), 0.5f);
    }

    private void Enables()
    {
        bodyHandler.LoseReferences();
        state.currentState = SpiderState.MovementState.Default;
        graphicsRenderer.gameObject.SetActive(true);
        graphics.gameObject.SetActive(true);
        bodyHandler.enabled = true;
        isDead = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 13)
            ResetPosition();
        else if (collision.gameObject.layer == 15)
            ResetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.GetChild(0).rotation * input * 5);
    }
}
