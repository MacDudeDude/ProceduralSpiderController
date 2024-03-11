using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebStrand : MonoBehaviour
{
    public LineRenderer line;
    public BoxCollider col;
    public Transform objectAttached;

    public bool seperated;

    private Vector3 objectAttachedPreviousPosition;

    void FixedUpdate()
    {
        if (!seperated)
        {
            if(objectAttached != null)
                objectAttachedPreviousPosition = objectAttached.position;

            return;
        }

        transform.position += objectAttached.position - objectAttachedPreviousPosition;
        objectAttachedPreviousPosition = objectAttached.position;
    }
}
