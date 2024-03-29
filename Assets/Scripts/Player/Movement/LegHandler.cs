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

    private Transform[] legRestingObjects;
    private Vector3[] legRestingObjectsPreviousPosition;
    private SpiderState state;
    private PlayerOrientation bodyManager;

    private bool isStepping;
    private int legGroupStepping;
    private float stepDurationLeft;

    private void Start()
    {
        state = transform.root.GetComponent<SpiderState>();
        bodyManager = GetComponentInChildren<PlayerOrientation>();

        for (int i = 0; i < legAnchors.Length; i++)
        {
            legAnchors[i].position = legTargets[i].position;
        }

        legRestingObjects = new Transform[legTargets.Length];
        legRestingObjectsPreviousPosition = new Vector3[legTargets.Length];
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
                UpdateLegPositions();
                AddMovingPlatformOffset();
                break;
            case SpiderState.MovementState.Default:
            default:
                UpdateLegPositions();
                AddMovingPlatformOffset();
                break;
        }
    }

    public float GetAverageLegHeight(float height)
    {
        float averageLegHeight = 0;

        for (int i = 0; i < legTargets.Length; i++)
        {
            averageLegHeight += bodyTransform.InverseTransformPoint(legTargets[i].position).y;
        }
        
        averageLegHeight /= legTargets.Length;

        return Mathf.Clamp01(averageLegHeight + height);
    }

    private void AddMovingPlatformOffset()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            if(legRestingObjects[i] != null)
            {
                Vector3 platformOffset = legRestingObjects[i].position - legRestingObjectsPreviousPosition[i];
                legRestingObjectsPreviousPosition[i] = legRestingObjects[i].position;

                legTargets[i].position = legTargets[i].position + platformOffset;
            }
        }
    }

    void MatchLegPositions()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            legTargets[i].position = Vector3.Lerp(legTargets[i].position, 
                legAnchors[i].position + (bodyTransform.position - legAnchors[i].position).normalized * ((i == 0 || i == 4) ? 0 : -1.5f) - Vector3.up * ((i == 0 || i == 4) ? 0 : 1), legMatchSpeed * Time.fixedDeltaTime);
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

    Vector3 GetNewLegPosition(Transform legPosition, Transform legAnchor, int legNum, bool clampVelocity = false)
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

        if(numColliders > 0)
        {
            if (legRestingObjects[legNum] != hitColliders[closestCollider].transform)
            {
                legRestingObjects[legNum] = hitColliders[closestCollider].transform;
                legRestingObjectsPreviousPosition[legNum] = hitColliders[closestCollider].transform.position;
            }

            Vector3 bodyUnitsPerSecond = (bodyManager.GetBodyVelocity() / Time.fixedDeltaTime) * stepDuration;
            Vector3 platformUnitsPerSecond = ((legRestingObjects[legNum].position - legRestingObjectsPreviousPosition[legNum]) / Time.fixedDeltaTime) * stepDuration;

            if(clampVelocity)
            {
                bodyUnitsPerSecond.x = Mathf.Clamp(bodyUnitsPerSecond.x, -1, 1);
                bodyUnitsPerSecond.x = Mathf.Clamp(bodyUnitsPerSecond.z, -1, 1);
                platformUnitsPerSecond.x = Mathf.Clamp(platformUnitsPerSecond.x, -1, 1);
                platformUnitsPerSecond.x = Mathf.Clamp(platformUnitsPerSecond.z, -1, 1);
            }

            return hitColliders[closestCollider].ClosestPoint(legAnchor.position + bodyUnitsPerSecond + platformUnitsPerSecond);
        }else
        {
            return legAnchor.position;
        }
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
                Vector3 newLegPosition = GetNewLegPosition(legTargets[i], legAnchors[i], i);
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
            if(legRestingObjects[i] != null)
                legRestingObjectsPreviousPosition[i] = legRestingObjects[i].position;
            Vector3 newLegPosition = GetNewLegPosition(legTargets[i], legAnchors[i], i, true);
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
