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

    [SerializeField] private float biteRange;
    [SerializeField] private float biteRadius;

    private bool biting;
    private float speed;
    private Vector3 defaultLeftValue;
    private Vector3 defaultRightValue;
    private Vector3 previousPosition;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

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
        Chomp();

        biting = true;
        StartCoroutine(Bite(leftFang, Quaternion.Euler(leftFangBiteRotation), biteDuration));
        StartCoroutine(Bite(rightFang, Quaternion.Euler(rightFangBiteRotation), biteDuration));
    }

    private void Chomp()
    {
        RaycastHit biteRay;
        if(Physics.SphereCast(cam.transform.position, biteRadius, cam.transform.forward, out biteRay, biteRange))
        {
            IBiteable biteable;
            if(biteRay.collider.gameObject.TryGetComponent(out biteable))
            {
                biteable.Bitten();
            }
        }
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

    private void OnDrawGizmos()
    {
        if(cam != null)
        {
            Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * biteRange);
            Gizmos.DrawWireSphere(cam.transform.position + cam.transform.forward * biteRange, biteRadius);
        }
    }
}
