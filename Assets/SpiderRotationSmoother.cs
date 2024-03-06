using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderRotationSmoother : MonoBehaviour
{
    [SerializeField] private Transform toMatch;
    [SerializeField] private float matchSpeed;
    [SerializeField] private bool matchPosition;

    private bool runInFixed;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            runInFixed = !runInFixed;

        if(!runInFixed && !matchPosition)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, toMatch.localRotation, 1f - Mathf.Exp(-matchSpeed * Time.deltaTime));
    }

    private void FixedUpdate()
    {
        if (matchPosition || runInFixed)
            transform.localRotation = Quaternion.Lerp(transform.localRotation, toMatch.localRotation, 1f - Mathf.Exp(-matchSpeed * Time.fixedDeltaTime));
        if(matchPosition)
            transform.position = toMatch.position;
    }
}
