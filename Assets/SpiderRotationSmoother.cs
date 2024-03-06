using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderRotationSmoother : MonoBehaviour
{
    [SerializeField] private Transform toMatch;
    [SerializeField] private float matchSpeed;
    [SerializeField] private bool matchPosition;

    private void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, toMatch.localRotation, 1f - Mathf.Exp(-matchSpeed * Time.deltaTime));
    }

    private void FixedUpdate()
    {
        if(matchPosition)
            transform.position = toMatch.position;
    }
}
