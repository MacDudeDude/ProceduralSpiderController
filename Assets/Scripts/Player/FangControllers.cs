using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FangControllers : MonoBehaviour
{
    [SerializeField] private Transform leftFang;
    [SerializeField] private Transform rightFang;

    [SerializeField] private float animMovement;
    [SerializeField] private float animSpeed;
    [SerializeField] private float speedModifier;

    private float speed;
    private Vector3 defaultLeftValue;
    private Vector3 defaultRightValue;
    private Vector3 previousPosition;

    private void Start()
    {
        defaultLeftValue = leftFang.localEulerAngles;
        defaultRightValue = rightFang.localEulerAngles;
    }

    private void Update()
    {
        float velocity = ((transform.position - previousPosition).magnitude / Time.deltaTime) * speedModifier;
        previousPosition = transform.position;

        velocity = Mathf.Clamp(velocity, 0, 2);
        speed = Mathf.Lerp(speed, velocity, Time.deltaTime * 0.2f);

        leftFang.localRotation = Quaternion.Euler(defaultLeftValue + Vector3.right * Mathf.Sin(Time.time * (animSpeed + speed)) * animMovement);
        rightFang.localRotation = Quaternion.Euler(defaultRightValue + Vector3.right * Mathf.Sin(Time.time * (animSpeed + speed)) * animMovement);
    }
}
