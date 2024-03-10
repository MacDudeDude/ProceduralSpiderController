using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private AnimationCurve easing;
    [SerializeField] private Vector3 startPositon;
    [SerializeField] private Vector3 toPosition;
    [SerializeField] private float duration;

    private void Start()
    {
        startPositon = transform.position;
        toPosition = transform.position + toPosition;
        StartCoroutine(MovePlatform());
        duration = Mathf.Clamp(duration, 1, Mathf.Infinity);
    }

    IEnumerator MovePlatform()
    {
        float time;
        while (enabled)
        {
            time = 0;
            while (time < duration)
            {
                transform.position = Vector3.Lerp(startPositon, toPosition, easing.Evaluate(time / duration));
                time += Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }
        }
    }
}
