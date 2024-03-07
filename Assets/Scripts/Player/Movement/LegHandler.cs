using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHandler : MonoBehaviour
{
    [Header("Step Settings")]
    [SerializeField] private float maxLegDistance;
    [SerializeField] private float legStepHeight;
    [SerializeField] private AnimationCurve legAnimationCurve;
    [SerializeField] private float stepDuration;

    [Header("Transforms")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform[] legTargets;
    [SerializeField] private Transform[] legAnchors;
    [SerializeField] private Vector4[] stepRays;
    [SerializeField] private Vector3[] stepRayPositions;

    [SerializeField] private LayerMask canStepLayers;

    private bool isStepping;
    private int legGroupStepping;
    private float stepDurationLeft;
    private Vector3 previousPosition;

    private void Start()
    {
        for (int i = 0; i < legAnchors.Length; i++)
        {
            legAnchors[i].position = legTargets[i].position;
        }
    }

    private void Update()
    {
        UpdateLegPositions();

        previousPosition = bodyTransform.position;
    }

    void UpdateLegPositions()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            if (Vector3.Distance(legTargets[i].position, legAnchors[i].position) > maxLegDistance)
            {
                if (LegCanStep(i))
                {
                    Vector3 newLegPosition = GetNewLegPosition(legTargets[i], legAnchors[i]);
                    StartCoroutine(MoveLeg(legTargets[i], newLegPosition, legTargets[i].up, stepDurationLeft));
                }
            }
        }

        if (isStepping)
        {
            stepDurationLeft -= Time.deltaTime;
            if(stepDurationLeft <= 0)
            {
                isStepping = false;
                legGroupStepping = legGroupStepping == 1 ? 0 : 1;
            }
        }
    }

    Vector3 GetNewLegPosition(Transform legPosition, Transform legAnchor)
    {
        RaycastHit hitInfo;
        if (Physics.Linecast(bodyTransform.position, legAnchor.position, out hitInfo, canStepLayers))
            return hitInfo.point;

        for (int r = 0; r < stepRays.Length; r++)
        {
            Vector3 rayPosition = legAnchor.position + bodyTransform.rotation * stepRayPositions[r];

            Vector3 rayDir = stepRays[r];
            rayDir = rayDir.normalized * stepRays[r].w;
            rayDir = bodyTransform.rotation * rayDir;

            if (Physics.Raycast(rayPosition, rayDir, out hitInfo, stepRays[r].w, canStepLayers))
                return hitInfo.point;
        }

        return legPosition.position;
    }

    int GetLegGroup(int legNumber)
    {
        if (legNumber >= legTargets.Length / 2)
            legNumber--;

        return legNumber % 2 == 0 ? 0 : 1;
    }

    bool LegCanStep(int legNumber)
    {
        if(isStepping)
        {
            if (stepDurationLeft < stepDuration / 2)
                return false;
            else
                return GetLegGroup(legNumber) == legGroupStepping;
        }

        if (GetLegGroup(legNumber) != legGroupStepping)
            return false;

        isStepping = true;
        stepDurationLeft = stepDuration;

        return true;

    }

    IEnumerator MoveLeg(Transform legTransform, Vector3 targetPosition, Vector3 legUp, float duration)
    {
        float time = 0;
        Vector3 startPosition = legTransform.position;

        while (time < duration)
        {
            float yOffset = legAnimationCurve.Evaluate(time / duration) * legStepHeight;
            legTransform.position = Vector3.Lerp(startPosition, targetPosition, time / duration) + legUp * yOffset;
            time += Time.deltaTime;

            yield return null;
        }

        legTransform.position = targetPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i < legAnchors.Length; i++)
        {
            Gizmos.DrawWireSphere(legAnchors[i].position, 0.25f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < legAnchors.Length; i++)
        {
            for (int r = 0; r < stepRays.Length; r++)
            {
                Vector3 rayPosition = legAnchors[i].position + bodyTransform.rotation * stepRayPositions[r];

                Vector3 rayDir = stepRays[r];
                rayDir = rayDir.normalized * stepRays[r].w;
                rayDir = bodyTransform.rotation * rayDir;

                Gizmos.DrawLine(rayPosition, rayPosition + rayDir);
            }
        }
    }
}
