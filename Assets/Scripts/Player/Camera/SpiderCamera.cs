using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCamera : MonoBehaviour
{
    [SerializeField] private Transform spiderHead;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }

    private void LateUpdate()
    {
        transform.position = spiderHead.position;
        transform.rotation = spiderHead.rotation;
    }
}
