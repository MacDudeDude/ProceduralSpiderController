using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCamera : MonoBehaviour
{
    [SerializeField] private Transform spiderHead;
    [SerializeField] private bool smooth;
    [SerializeField] private float smoothness;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }

    private void LateUpdate()
    {
        if(smooth)
        {
            transform.position = Vector3.Lerp(transform.position, spiderHead.position, smoothness * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, spiderHead.rotation, smoothness * Time.deltaTime);
        }
        else
        {
            transform.position = spiderHead.position;
            transform.rotation = spiderHead.rotation;
        }
    }
}
