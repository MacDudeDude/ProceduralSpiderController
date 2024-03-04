using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float counterforceStrength;

    private Rigidbody rb;
    private Vector3 input;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GetInputs();
    }

    private void FixedUpdate()
    {
        Move();
        CounterMove();
    }

    private void GetInputs()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.z = Input.GetAxisRaw("Vertical");
        input.Normalize();
    }

    private void Move()
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);
        if (Mathf.Abs(relativeVelocity.x) >= maxSpeed && relativeVelocity.x > 0 == input.x > 0)
            input.x = 0;
        if (Mathf.Abs(relativeVelocity.z) >= maxSpeed && relativeVelocity.z > 0 == input.z > 0)
            input.z = 0;

        rb.AddRelativeForce(input * speed);
    }

    private void CounterMove()
    {
        Vector3 counterMovement = Vector3.zero;
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);

        if(Mathf.Approximately(input.x, 0))
            counterMovement.x = relativeVelocity.x;
        if (Mathf.Approximately(input.z, 0))
            counterMovement.z = relativeVelocity.z;

        counterMovement *= -1;

        rb.AddRelativeForce(counterMovement * counterforceStrength);
    }
}
