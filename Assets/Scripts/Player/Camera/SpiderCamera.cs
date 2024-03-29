using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCamera : MonoBehaviour
{
    [SerializeField] private Transform spiderHead;
    [SerializeField] private bool smooth;
    [SerializeField] private float smoothness;
    [SerializeField] private int camNumber;

    private int activeCamera;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            activeCamera++;
            if (activeCamera > 1)
                activeCamera = 0;
            transform.GetChild(0).gameObject.SetActive(camNumber == activeCamera);
        }
    }

    private void LateUpdate()
    {
        if (!smooth)
        {
            transform.position = spiderHead.position;
            transform.rotation = spiderHead.rotation;
        }
    }

    private void FixedUpdate()
    {
        if (smooth)
        {
            transform.position = Vector3.Lerp(transform.position, spiderHead.position, smoothness * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, spiderHead.rotation, smoothness * Time.fixedDeltaTime);
        }
    }
}
