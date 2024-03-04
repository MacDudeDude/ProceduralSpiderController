using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCamera : MonoBehaviour
{
    [SerializeField] private Transform spiderHead;

    void LateUpdate()
    {
        transform.position = spiderHead.position;
        transform.rotation = spiderHead.rotation;
    }
}
