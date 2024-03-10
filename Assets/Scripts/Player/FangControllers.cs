using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FangControllers : MonoBehaviour
{
    [SerializeField] private Transform leftFang;
    [SerializeField] private Transform rightFang;
    [SerializeField] private Vector3 leftFangBiteRotation;
    [SerializeField] private Vector3 rightFangBiteRotation;

    [SerializeField] private float biteDuration;
    [SerializeField] private AnimationCurve biteCurve;

    [SerializeField] private float animMovement;
    [SerializeField] private float animSpeed;
    [SerializeField] private float speedModifier;

    private bool biting;
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
        if(!biting)
            IdleAnimation();
    }

    public void Bite()
    {
        StopAllCoroutines();

        biting = true;
        StartCoroutine(Bite(leftFang, Quaternion.Euler(leftFangBiteRotation), biteDuration));
        StartCoroutine(Bite(rightFang, Quaternion.Euler(rightFangBiteRotation), biteDuration));
    }

    private void IdleAnimation()
    {
        float velocity = ((transform.position - previousPosition).magnitude / Time.deltaTime) * speedModifier;
        previousPosition = transform.position;

        velocity = Mathf.Clamp(velocity, 0, 2);
        speed = Mathf.Lerp(speed, velocity, Time.deltaTime * 0.2f);

        leftFang.localRotation = Quaternion.Euler(defaultLeftValue + Vector3.right * Mathf.Sin(Time.time * (animSpeed + speed)) * animMovement);
        rightFang.localRotation = Quaternion.Euler(defaultRightValue + Vector3.right * Mathf.Sin(Time.time * (animSpeed + speed)) * animMovement);
    }

    IEnumerator Bite(Transform fang, Quaternion targetRotation, float duration)
    {
        float time = 0;
        Quaternion startRotation = Quaternion.Euler(fang.localRotation.eulerAngles);

        while (time < duration)
        {
            fang.localRotation = Quaternion.LerpUnclamped(startRotation, targetRotation, biteCurve.Evaluate(time / duration));
            time += Time.deltaTime;

            yield return null;
        }

        time = 0;
        fang.localRotation = targetRotation;

        targetRotation = startRotation;
        startRotation = fang.localRotation;

        while (time < duration)
        {
            fang.localRotation = Quaternion.LerpUnclamped(startRotation, targetRotation, biteCurve.Evaluate(time / duration));
            time += Time.deltaTime;

            yield return null;
        }

        fang.localRotation = targetRotation;
        biting = false;
    }
}
