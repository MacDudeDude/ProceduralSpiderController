using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWebManager : MonoBehaviour
{
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform transformBody;
    [SerializeField] private Transform backBody;
    [SerializeField] private LayerMask blockableLayers;

    private SpiderState state;
    private PlayerOrientation bodyManager;

    private WebStrand currentWeb;
    private WebStrand currentDescendWeb;

    private bool wasWebbing;
    private bool wasDescending;
    private bool hitObstacle;

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<SpiderState>();
        bodyManager = GetComponentInChildren<PlayerOrientation>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LineWebbing();
        DescendWebbing();
    }

    private void LineWebbing()
    {
        if (state.isWebbing)
        {
            if (!wasWebbing)
            {
                currentWeb = Instantiate(webPrefab).GetComponent<WebStrand>();

                currentWeb.isWeb = true;
                currentWeb.line.enabled = true;

                currentWeb.objectAttached1 = bodyManager.GetPlatformObject();

                currentWeb.startPosition = bodyManager.GetGroundPoint();
                currentWeb.SetPositions();

                wasWebbing = true;
                hitObstacle = false;
            }

            currentWeb.endPosition = backBody.position;

            RaycastHit lineCheck;
            if(Physics.Linecast(currentWeb.startPosition, currentWeb.endPosition, out lineCheck, blockableLayers))
            {
                state.isWebbing = false;
                hitObstacle = true;

                currentWeb.endPosition = lineCheck.point;
                currentWeb.line.SetPosition(1, lineCheck.point);
                currentWeb.objectAttached2 = lineCheck.collider.transform;
            }

        }
        else if (wasWebbing)
        {
            if (currentWeb != null)
            {
                if(!hitObstacle)
                {
                    currentWeb.endPosition = bodyManager.GetGroundPoint();
                    currentWeb.line.SetPosition(1, bodyManager.GetGroundPoint());
                    currentWeb.objectAttached2 = bodyManager.GetPlatformObject();
                }

                currentWeb.CalculateCollider(currentWeb.startPosition, currentWeb.endPosition);

                currentWeb.seperated = true;
                currentWeb.col.enabled = true;

                currentWeb.SetPositions();
                currentWeb = null;
            }

            wasWebbing = false;
        }
    }

    private void DescendWebbing()
    {
        if (state.currentState == SpiderState.MovementState.Descending)
        {
            if (!wasDescending)
            {
                currentDescendWeb = Instantiate(webPrefab).GetComponent<WebStrand>();
                currentDescendWeb.line.enabled = true;
                currentDescendWeb.objectAttached1 = bodyManager.GetPlatformObject();

                currentDescendWeb.startPosition = bodyManager.GetGroundPoint();
                wasDescending = true;
            }

            currentDescendWeb.startPosition += bodyManager.GetPlatformOffset();
            currentDescendWeb.endPosition = currentDescendWeb.startPosition;
            currentDescendWeb.endPosition.y = transformBody.position.y;

            currentDescendWeb.line.SetPosition(0, currentDescendWeb.startPosition);
            currentDescendWeb.line.SetPosition(1, currentDescendWeb.endPosition);

        }
        else if (wasDescending)
        {
            if (currentDescendWeb != null)
            {
                currentDescendWeb.col.enabled = true;
                currentDescendWeb.line.useWorldSpace = false;

                currentDescendWeb.col.center = currentDescendWeb.line.bounds.center;
                currentDescendWeb.col.size = currentDescendWeb.line.bounds.size;

                currentDescendWeb.seperated = true;

                currentDescendWeb.SetPositions();
                currentDescendWeb = null;
            }

            wasDescending = false;
        }
    }
}
