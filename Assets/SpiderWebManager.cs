using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWebManager : MonoBehaviour
{
    [SerializeField] private GameObject webPrefab;

    [SerializeField] private Transform spiderBody;

    private SpiderState state;
    private PlayerOrientation bodyManager;
    private WebStrand currentWeb;
    private Vector3 webStartPosition;
    private bool isDescending;

    // Start is called before the first frame update
    void Start()
    {
        state = GetComponent<SpiderState>();
        bodyManager = GetComponentInChildren<PlayerOrientation>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state.currentState == SpiderState.MovementState.Descending)
        {
            if (!isDescending)
            {
                currentWeb = Instantiate(webPrefab).GetComponent<WebStrand>();
                currentWeb.line.enabled = true;
                currentWeb.objectAttached = bodyManager.GetPlatformObject();

                webStartPosition = bodyManager.GetGroundPoint();
                isDescending = true;
            }

            webStartPosition += bodyManager.GetPlatformOffset();
            Vector3 webEndPosition = webStartPosition;
            webEndPosition.y = spiderBody.position.y;

            currentWeb.line.SetPosition(0, webStartPosition);
            currentWeb.line.SetPosition(1, webEndPosition);

        }
        else if(isDescending)
        {
            if(currentWeb != null)
            {
                currentWeb.col.enabled = true;
                currentWeb.col.center = currentWeb.line.bounds.center;
                currentWeb.col.size = currentWeb.line.bounds.size + new Vector3(currentWeb.line.bounds.size.x, 0, currentWeb.line.bounds.size.z);

                currentWeb.seperated = true;

                currentWeb = null;
            }

            isDescending = false;
        }
    }
}
