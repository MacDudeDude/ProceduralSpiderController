using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegHandler : MonoBehaviour
{
    [SerializeField] private float maxLegDistance;
    [SerializeField] private float legStepHeight;
    [SerializeField] private AnimationCurve legAnimationCurve;
    [SerializeField] private float stepDuration;
    [SerializeField] private Transform[] legTargets;
    [SerializeField] private Transform[] legAnchors;

    private bool isStepping;
    private int legGroupStepping;
    private float stepDurationLeft;

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
        return Vector3.LerpUnclamped(legPosition.position, legAnchor.position, 1.5f);
    }

    int GetLegGroup(int legNumber)
    {
        if (legNumber > legTargets.Length / 2)
            legNumber -= legTargets.Length / 2;

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
}
