using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
	[SerializeField] private Transform headTransform;
	[SerializeField] private Transform bodyTransform;
	[SerializeField] private float headYOffset;

	[SerializeField] [Range(0f, 90f)] private float yRotationLimit = 88f;
	[SerializeField] [Range(0.1f, 9f)] private float sensitivity = 2f;

	Vector2 rotation;

	private void Start()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		Application.targetFrameRate = 30;
	}

    void Update()
    {
        rotation.x += Input.GetAxis("Mouse X") * sensitivity;
        rotation.y += Input.GetAxis("Mouse Y") * sensitivity;

        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit + headYOffset, yRotationLimit + headYOffset);

        Quaternion bodyRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);
        Quaternion headRotation = Quaternion.AngleAxis(rotation.y, Vector3.left);

        bodyTransform.localRotation = bodyRotation;
        headTransform.localRotation = headRotation;
    }
}
