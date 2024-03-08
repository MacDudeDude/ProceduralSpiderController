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
    [SerializeField] private float legMatchSpeed;

    [Header("Transforms")]
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Transform[] legTargets;
    [SerializeField] private Transform[] legAnchors;
    [SerializeField] private float stepCollisionRadius;
    [SerializeField] private int maxStepCollisionsCheck;

    [SerializeField] private LayerMask canStepLayers;

    [Header("Debugging")]
    [SerializeField] private bool debug;

    private SpiderState state;

    private bool isStepping;
    private int legGroupStepping;
    private float stepDurationLeft;

    private void Start()
    {
        state = transform.root.GetComponent<SpiderState>();

        for (int i = 0; i < legAnchors.Length; i++)
        {
            legAnchors[i].position = legTargets[i].position;
        }
    }

    private void FixedUpdate()
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Jumping:
            case SpiderState.MovementState.Falling:
                MatchLegPositions();
                break;
            case SpiderState.MovementState.Descending:
            case SpiderState.MovementState.Default:
            default:
                UpdateLegPositions();
                break;
        }
    }

    void MatchLegPositions()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            legTargets[i].position = Vector3.Lerp(legTargets[i].position, 
                legAnchors[i].position + (bodyTransform.position - legAnchors[i].position).normalized * -1.5f - Vector3.up * 1, legMatchSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateLegPositions()
    {
        if(!isStepping)
        {
            for (int i = 0; i < legTargets.Length; i++)
            {
                if(GetLegGroup(i) == legGroupStepping)
                {
                    if (Vector3.Distance(legTargets[i].position, legAnchors[i].position) > maxLegDistance)
                    {
                        MoveLegGroup(i);
                        break;
                    }
                }
            }

            if(!isStepping)
            {
                for (int i = 0; i < legTargets.Length; i++)
                {
                    if (GetLegGroup(i) != legGroupStepping)
                    {
                        if (Vector3.Distance(legTargets[i].position, legAnchors[i].position) > maxLegDistance)
                        {
                            MoveLegGroup(i);
                            break;
                        }
                    }
                }
            }

        }

        if (isStepping)
        {
            stepDurationLeft -= Time.fixedDeltaTime;
            if(stepDurationLeft <= 0)
            {
                isStepping = false;
                legGroupStepping = legGroupStepping == 1 ? 0 : 1;
            }
        }
    }

    Vector3 GetNewLegPosition(Transform legPosition, Transform legAnchor)
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Descending:
                if(!state.isGrounded)
                    return bodyTransform.position + Vector3.up * 3;
                break;
            case SpiderState.MovementState.Default:
            default:
                break;
        }

        Collider[] hitColliders = new Collider[maxStepCollisionsCheck];
        int numColliders = Physics.OverlapSphereNonAlloc(legAnchor.position, stepCollisionRadius, hitColliders, canStepLayers);

        float searchSize = stepCollisionRadius;
        while(numColliders == 0)
        {
            searchSize += stepCollisionRadius;
            numColliders = Physics.OverlapSphereNonAlloc(legAnchor.position, searchSize, hitColliders, canStepLayers);

            if (searchSize > stepCollisionRadius * 5)
                break;
        }

        int closestCollider = 0;
        float closestPoint = Mathf.Infinity;
        for (int i = 0; i < numColliders; i++)
        {
            Vector3 hitPoint = hitColliders[i].ClosestPoint(legAnchor.position);
            float distance = (legAnchor.position - hitPoint).magnitude;

            if(distance < closestPoint)
            {
                closestPoint = distance;
                closestCollider = i;
            }
        }

        return numColliders > 0 ? hitColliders[closestCollider].ClosestPoint(legAnchor.position) : legAnchor.position;
    }

    int GetLegGroup(int legNumber)
    {
        if (legNumber >= legTargets.Length / 2)
            legNumber--;

        return legNumber % 2 == 0 ? 0 : 1;
    }

    void MoveLegGroup(int legNumber)
    {
        isStepping = true;
        stepDurationLeft = stepDuration;

        legGroupStepping = GetLegGroup(legNumber);
        for (int i = 0; i < legTargets.Length; i++)
        {
            if(GetLegGroup(i) == legGroupStepping)
            {
                Vector3 newLegPosition = GetNewLegPosition(legTargets[i], legAnchors[i]);
                StartCoroutine(MoveLeg(legTargets[i], newLegPosition, bodyTransform.up, stepDurationLeft));
            }
        }
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

    public void ForceMoveAllLegs()
    {
        isStepping = true;
        stepDurationLeft = stepDuration;

        for (int i = 0; i < legTargets.Length; i++)
        {
            Vector3 newLegPosition = GetNewLegPosition(legTargets[i], legAnchors[i]);
            StartCoroutine(MoveLeg(legTargets[i], newLegPosition, bodyTransform.up, stepDurationLeft));
        }
    }

    private void OnDrawGizmos()
    {
        if(!debug)
            return;

        Gizmos.color = Color.white;
        for (int i = 0; i < legAnchors.Length; i++)
        {
            Gizmos.DrawWireSphere(legAnchors[i].position, 0.25f);
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < legAnchors.Length; i++)
        {
            Gizmos.DrawWireSphere(legAnchors[i].position, stepCollisionRadius);
        }
    }
}
