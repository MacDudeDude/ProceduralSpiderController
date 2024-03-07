using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrientation : MonoBehaviour
{
    [SerializeField] private Transform absoulteRotationTransform;

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckRadius;

    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float wallCheckRadius;

    [SerializeField] private LayerMask walkableLayers;

    [SerializeField] private float heightMatchSpeed;
    [SerializeField] private float rotationMatchSpeed;
    [SerializeField] private float height;
    [SerializeField] private float breathingSpeed;
    [SerializeField] private float breathingStrength;

    [SerializeField] private bool useVelocityForWallCheck;

    private Rigidbody rb;
    private SpiderState state;

    private Vector3 inputVector;

    private Vector3 groundPoint;
    private Vector3 wallPoint;

    private Vector3 groundNormal;
    private Vector3 wallNormal;
    private float distanceToWall;

    private Vector3 previousPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        state = transform.root.GetComponent<SpiderState>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputVector();
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckWall();
        SetUpRotation();
    }

    public Vector3 GetNewHeightPosition()
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Jumping:
                return Vector3.zero;
            case SpiderState.MovementState.Descending:
                return rb.position - Vector3.up * 4f * Time.fixedDeltaTime;

            case SpiderState.MovementState.Default:
            default:
                return Vector3.Lerp(rb.position, groundPoint + Vector3.Slerp(groundNormal, wallNormal, distanceToWall) * (height + Mathf.Sin(Time.time * breathingSpeed) * breathingStrength), heightMatchSpeed * Time.fixedDeltaTime);
        }
    }

    private void SetUpRotation()
    {
        Quaternion targetRotation = SpiderUpRotation(transform.forward, Vector3.Slerp(groundNormal, wallNormal, distanceToWall));
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationMatchSpeed * Time.fixedDeltaTime));
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
        if(useVelocityForWallCheck)
        {
            Vector3 newInputVector = absoulteRotationTransform.InverseTransformVector((absoulteRotationTransform.position - previousPosition));
            if (VectorIsNotZero(newInputVector))
            {
                newInputVector.y = 0;
                inputVector = newInputVector.normalized;
                inputVector = absoulteRotationTransform.TransformDirection(inputVector);
            }
            previousPosition = absoulteRotationTransform.position;
        }else
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                inputVector.x = Input.GetAxisRaw("Horizontal");
                inputVector.y = 0;
                inputVector.z = Input.GetAxisRaw("Vertical");
                inputVector = absoulteRotationTransform.TransformDirection(inputVector);
            }

            if(state.currentState == SpiderState.MovementState.Descending)
            {
                inputVector.x = 0;
                inputVector.y = -1;
                inputVector.z = 0;
                //inputVector = absoulteRotationTransform.TransformDirection(inputVector);
            }
        }
    }

    private bool VectorIsNotZero(Vector3 toCheck)
    {
        if (Mathf.Abs(toCheck.x) > 0.05f)
            return true;
        if (Mathf.Abs(toCheck.z) > 0.05f)
            return true;

        return false;
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
            Gizmos.DrawSphere(hitInfo.point, wallCheckRadius);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + (inputVector * wallCheckDistance), wallCheckRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.Slerp(groundNormal, wallNormal, distanceToWall));
    }
}
