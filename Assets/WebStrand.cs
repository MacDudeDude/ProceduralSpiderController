using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebStrand : MonoBehaviour
{
    public LineRenderer line;
    public BoxCollider col;
    public Transform objectAttached1;
    public Transform objectAttached2;
    public Vector3 startPosition;
    public Vector3 endPosition;

    public bool seperated;
    public bool isWeb;

    private Vector3 objectAttachedPreviousPosition1;
    private Vector3 objectAttachedPreviousPosition2;

    void FixedUpdate()
    {
        if (isWeb)
        {
            UpdateWebbing();
        }else if(seperated)
        {
            UpdateDescendStrand();
        }
    }

    private void UpdateDescendStrand()
    {
        if (objectAttached1 != null)
        {
            transform.position += objectAttached1.position - objectAttachedPreviousPosition1;
            objectAttachedPreviousPosition1 = objectAttached1.position;
        }
    }

    private void UpdateWebbing()
    {
        if (objectAttached1 != null)
        {
            startPosition += objectAttached1.position - objectAttachedPreviousPosition1;
            objectAttachedPreviousPosition1 = objectAttached1.position;
        }

        if (seperated && objectAttached2 != null)
        {
            endPosition += objectAttached2.position - objectAttachedPreviousPosition2;
            objectAttachedPreviousPosition2 = objectAttached2.position;
        }

        line.SetPosition(0, startPosition);
        line.SetPosition(1, endPosition);
        CalculateCollider(startPosition, endPosition);
    }

    public void SetPositions()
    {
        if (objectAttached1 != null)
            objectAttachedPreviousPosition1 = objectAttached1.position;

        if (objectAttached2 != null)
            objectAttachedPreviousPosition2 = objectAttached2.position;
    }

    public void CalculateCollider(Vector3 startPos, Vector3 endPos)
    {
        transform.position = (startPos + endPos) / 2f;

        col.size = new Vector3(0.1f, Vector3.Distance(startPos, endPos), 0.1f);

        transform.up = endPos - startPos;
    }
}
