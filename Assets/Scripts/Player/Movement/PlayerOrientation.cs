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

    [SerializeField] private float maxFallSpeed = 20f;

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
    private float fallingSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        CheckGround();
        CheckWall();
        SetUpRotation();
    }

    public Vector3 GetNewHeightPosition()
    {
        switch (state.currentState)
        {
            case SpiderState.MovementState.Descending:
                return rb.position - Vector3.up * 5f * -inputVector.y * Time.fixedDeltaTime;
            case SpiderState.MovementState.Jumping:
            case SpiderState.MovementState.Falling:
                return rb.position - Vector3.Lerp(groundNormal, Vector3.up, Mathf.InverseLerp(-maxFallSpeed, maxFallSpeed, fallingSpeed)) * fallingSpeed * Time.fixedDeltaTime;
            case SpiderState.MovementState.Default:
            default:
                return Vector3.Lerp(rb.position, groundPoint + Vector3.Slerp(groundNormal, wallNormal, distanceToWall) * (height + Mathf.Sin(Time.time * breathingSpeed) * breathingStrength), heightMatchSpeed * Time.fixedDeltaTime);
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

    private void CheckGround()
    {
        if (state.currentState != SpiderState.MovementState.Jumping)
            return;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, groundCheckRadius, -transform.up, out hitInfo, groundCheckDistance, walkableLayers))
        {
            groundPoint = hitInfo.point;
            groundNormal = hitInfo.normal;
        }
        else
        {
            groundPoint = wallPoint;
            groundNormal = wallNormal;
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
                Vector3 newInputVector = absoulteRotationTransform.position - previousPosition;
                if (VectorIsNotZero(newInputVector))
                {
                    inputVector = newInputVector.normalized;
                }
                previousPosition = absoulteRotationTransform.position;
                break;
            case SpiderState.MovementState.Default:
            default:
                if (useVelocityForWallCheck)
                {
                    newInputVector = absoulteRotationTransform.position - previousPosition;
                    if (VectorIsNotZero(newInputVector))
                    {
                        inputVector = newInputVector.normalized;
                    }
                    previousPosition = absoulteRotationTransform.position;
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
        StopAllCoroutines();
        StartCoroutine(AnimateFallingSpeed(fallingSpeed * -1, 1));
    }

    public void Land()
    {

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
