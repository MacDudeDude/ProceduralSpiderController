using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;

    private PlayerOrientation bodyHandler;
    private Rigidbody rb;
    private Vector3 input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyHandler = GetComponent<PlayerOrientation>();
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
    }

    private void Move()
    {
        Vector3 newPosition = bodyHandler.GetNewHeightPosition();
        Vector3 forces = transform.GetChild(0).rotation * input * speed;
        rb.MovePosition(newPosition + forces * Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, transform.GetChild(0).rotation * input * 5);
    }
}
