using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrientation : MonoBehaviour
{
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckRadius;

    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float wallCheckRadius;

    [SerializeField] private LayerMask walkableLayers;

    [SerializeField] private float height;

    private Rigidbody rb;

    private Vector3 inputVector;

    private Vector3 groundPoint;
    private Vector3 wallPoint;

    private Vector3 groundNormal;
    private Vector3 wallNormal;
    private float distanceToWall;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputVector();

        CheckGround();
        CheckWall();
    }

    private void FixedUpdate()
    {
        SetUpRotation();
    }

    public Vector3 GetNewHeightPosition()
    {
        return Vector3.Lerp(rb.position, groundPoint + Vector3.Lerp(groundNormal, wallNormal, distanceToWall) * height, 2 * Time.fixedDeltaTime);
    }

    private void SetUpRotation()
    {
        var targetRotation = SpiderUpRotation(transform.forward, Vector3.Slerp(groundNormal, wallNormal, distanceToWall));
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 20 * Time.fixedDeltaTime));
    }

    private void CheckGround()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out hitInfo, groundCheckDistance, walkableLayers))
        {
            groundPoint = hitInfo.point;
            groundNormal = hitInfo.normal;
        }
        else
        {

        }
    }

    private void CheckWall()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, wallCheckRadius, inputVector, out hitInfo, wallCheckDistance, walkableLayers))
        {
            wallPoint = hitInfo.point;
            wallNormal = hitInfo.normal;
            distanceToWall = (wallCheckDistance - hitInfo.distance) / wallCheckDistance;
        }
        else
        {
            distanceToWall = 0;
        }
    }

    private void GetInputVector()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            inputVector.x = Input.GetAxisRaw("Horizontal");
            inputVector.y = 0;
            inputVector.z = Input.GetAxisRaw("Vertical");
            inputVector = transform.TransformDirection(inputVector);
        }

    }

    Quaternion SpiderUpRotation(Vector3 approximateForward, Vector3 exactUp)
    {
        Quaternion zToUp = Quaternion.LookRotation(exactUp, -approximateForward);
        Quaternion yToz = Quaternion.Euler(90, 0, 0);
        return zToUp * yToz;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out hitInfo, groundCheckDistance, walkableLayers))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitInfo.point, groundCheckRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + (-transform.up * groundCheckDistance), groundCheckRadius);
        }

        Gizmos.DrawWireSphere(transform.position, wallCheckRadius);
        if (Physics.SphereCast(transform.position, wallCheckRadius, inputVector, out hitInfo, wallCheckDistance, walkableLayers))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(hitInfo.point, wallCheckRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + (inputVector * wallCheckDistance), wallCheckRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.Slerp(groundNormal, wallNormal, distanceToWall));
    }
}
