using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrientation : MonoBehaviour
{
    [SerializeField] private Transform absoulteRotationTransform;

    [SerializeField] private float groundCheckLength;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckRadius;

    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float wallCheckRadius;

    [SerializeField] private LayerMask walkableLayers;

    [SerializeField] private float descendSpeed = 12f;
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float walkEffectStrength = 4f;

    [SerializeField] private float heightMatchSpeed;
    [SerializeField] private float rotationMatchSpeed;
    [SerializeField] private float height;
    [SerializeField] private float breathingSpeed;
    [SerializeField] private float breathingStrength;

    [SerializeField] private bool useVelocityForWallCheck;

    private Rigidbody rb;
    private LegHandler legHandler;
    private SpiderState state;

    private Vector3 inputVector;

    private Transform groundObject;
    private Vector3 groundObjectOffset;
    private Vector3 groundObjectPreviousPosition;

    private Vector3 groundPoint;
    private Vector3 groundNormal;
    private Vector3 wallPoint;
    private Vector3 wallNormal;

    private float distanceToWall;

    private Vector3 bodyVelocity;
    private Vector3 previousPosition;
    private float fallingSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        legHandler = GetComponentInParent<LegHandler>();
        state = transform.root.GetComponent<SpiderState>();

        wallNormal = Vector3.up;
        wallPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputVector();
    }

    private void FixedUpdate()
    {
        CalculateStuff();
        CheckGround();
        CheckWall();
        SetUpRotation();
    }

    public Vector3 GetNewHeightPosition()
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Descending:
                return rb.position - Vector3.up * descendSpeed * -inputVector.y * Time.fixedDeltaTime;
            case SpiderState.MovementState.Falling:
                return rb.position - Vector3.up * maxFallSpeed * Time.fixedDeltaTime;
            case SpiderState.MovementState.Jumping:
                return rb.position - Vector3.Lerp(groundNormal, Vector3.up, Mathf.InverseLerp(-maxFallSpeed, maxFallSpeed, fallingSpeed)) * fallingSpeed * Time.fixedDeltaTime;
            case SpiderState.MovementState.Default:
            default:
                return Vector3.Lerp(rb.position, groundPoint + Vector3.Slerp(groundNormal, wallNormal, distanceToWall) * 
                    (height + Mathf.Sin(Time.time * breathingSpeed) * breathingStrength + legHandler.GetAverageLegHeight(height) * walkEffectStrength), 
                    heightMatchSpeed * Time.fixedDeltaTime);
        }
    }

    private void SetUpRotation()
    {
        Quaternion targetRotation = SpiderUpRotation(transform.forward, Vector3.Slerp(groundNormal, wallNormal, distanceToWall));

        switch (state.currentState)
        {
            case SpiderState.MovementState.Falling:
                targetRotation = SpiderUpRotation(transform.forward, Vector3.Lerp(transform.up, Vector3.up, 15 * Time.fixedDeltaTime));
                break;
            case SpiderState.MovementState.Jumping:
                targetRotation = SpiderUpRotation(transform.forward, transform.up);
                break;
            default:
                break;
        }

        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationMatchSpeed * Time.fixedDeltaTime));
    }

    private void CalculateStuff()
    {
        bodyVelocity = absoulteRotationTransform.position - previousPosition;
        previousPosition = absoulteRotationTransform.position;
    }

    private void CheckGround()
    {
        if (state.currentState == SpiderState.MovementState.Jumping)
            return;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out hitInfo, groundCheckDistance, walkableLayers))
        {
            groundPoint = hitInfo.point;
            groundNormal = hitInfo.normal;

            if(groundObject == null || groundObject != hitInfo.transform)
            {
                groundObject = hitInfo.transform;
                groundObjectPreviousPosition = groundObject.position;
            }

            groundObjectOffset = groundObject.position - groundObjectPreviousPosition;
            groundObjectPreviousPosition = groundObject.position;
        }
        else
        {
            groundPoint = wallPoint;
            groundNormal = wallNormal;

            if(state.currentState != SpiderState.MovementState.Descending)
            {
                groundObject = null;
                groundObjectOffset = Vector3.zero;
            }else
            {
                if(groundObject != null)
                {
                    groundObjectOffset = groundObject.position - groundObjectPreviousPosition;
                    groundObjectPreviousPosition = groundObject.position;
                }
            }
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
        switch (state.currentState)
        {
            case SpiderState.MovementState.Descending:
                inputVector.x = 0;
                inputVector.y = -Input.GetAxisRaw("Vertical");
                inputVector.z = 0;
                break;
            case SpiderState.MovementState.Falling:
            case SpiderState.MovementState.Jumping:
                if (VectorIsNotZero(bodyVelocity))
                {
                    inputVector = bodyVelocity.normalized;
                }
                break;
            case SpiderState.MovementState.Default:
            default:
                if (useVelocityForWallCheck)
                {
                    if (VectorIsNotZero(bodyVelocity))
                    {
                        inputVector = bodyVelocity.normalized;
                    }
                }
                else
                {
                    if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                    {
                        inputVector.x = Input.GetAxisRaw("Horizontal");
                        inputVector.y = 0;
                        inputVector.z = Input.GetAxisRaw("Vertical");
                        inputVector = absoulteRotationTransform.TransformDirection(inputVector);
                    }
                }
                break;
        }
    }

    private bool VectorIsNotZero(Vector3 toCheck)
    {
        if (Mathf.Abs(toCheck.x) > 0.05f)
            return true;
        if (Mathf.Abs(toCheck.y) > 0.05f)
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

    public bool IsGrounded(out bool spiderOnGroundGround)
    {
        spiderOnGroundGround = Vector3.Dot(transform.up, Vector3.up) > 0.9f;

        RaycastHit hitInfo;
        return Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out hitInfo, groundCheckLength, walkableLayers);
    }

    public bool WallGrounded()
    {
        RaycastHit hitInfo;
        return Physics.SphereCast(transform.position, wallCheckRadius, inputVector, out hitInfo, groundCheckLength, walkableLayers);
    }

    public void Jump()
    {
        fallingSpeed = -maxFallSpeed;

        groundObject = null;

        StopAllCoroutines();
        StartCoroutine(AnimateFallingSpeed(fallingSpeed * -1, jumpDuration));
    }

    public void Land()
    {

    }

    public Vector3 GetGroundPoint()
    {
        return groundPoint;
    }

    public Transform GetPlatformObject()
    {
        return groundObject;
    }

    public Vector3 GetPlatformOffset()
    {
        return groundObjectOffset;
    }

    public Vector3 GetBodyVelocity()
    {
        return bodyVelocity;
    }

    IEnumerator AnimateFallingSpeed(float endValue, float duration)
    {
        float time = 0;
        float startValue = fallingSpeed;

        while (time < duration)
        {
            fallingSpeed = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        fallingSpeed = endValue;
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
