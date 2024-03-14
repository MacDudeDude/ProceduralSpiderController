using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private AnimationCurve easing;
    [SerializeField] private Vector3 startPositon;
    [SerializeField] private Vector3 toPosition;
    [SerializeField] private float duration;
    [SerializeField] private bool hardSet;
    [SerializeField] private bool rotation;
    [SerializeField] private float startDelay;
    [SerializeField] private bool justRotate;

    private void Start()
    {
        if (justRotate)
        {
            toPosition.Normalize();
            return;
        }

        if(!hardSet)
        {
            startPositon = transform.localPosition;
            toPosition = transform.localPosition + toPosition;
        }
        if(!rotation)
            StartCoroutine(MovePlatform());
        else
            StartCoroutine(RotatePlatform());

        duration = Mathf.Clamp(duration, 1, Mathf.Infinity);
    }

    private void Update()
    {
        if (justRotate && Time.realtimeSinceStartup > startDelay)
            transform.Rotate(toPosition * Time.deltaTime * duration);
    }

    IEnumerator MovePlatform()
    {
        yield return new WaitForSeconds(startDelay);

        float time;
        while (enabled)
        {
            time = 0;
            while (time < duration)
            {
                transform.localPosition = Vector3.LerpUnclamped(startPositon, toPosition, easing.Evaluate(time / duration));
                time += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }
            duration = Mathf.Clamp(duration, 1, Mathf.Infinity);
        }
    }
    IEnumerator RotatePlatform()
    {
        float time;
        while (enabled)
        {
            time = 0;
            while (time < duration)
            {
                transform.localRotation = Quaternion.LerpUnclamped(Quaternion.Euler(startPositon), Quaternion.Euler(toPosition), easing.Evaluate(time / duration));
                time += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }
            duration = Mathf.Clamp(duration, 1, Mathf.Infinity);
        }
    }
}
