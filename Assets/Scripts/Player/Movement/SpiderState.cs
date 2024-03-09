using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderState : MonoBehaviour
{
    public enum MovementState
    {
        Default,
        Jumping,
        Descending,
        Falling
    }

    private bool isDescending;
    private bool isJumping;
    private bool isFalling;
    public bool isGrounded;
    public bool onGroundGround;
    public bool wallDetected;
    private float jumpBuffer;

    public bool lockInputForces;
    public MovementState currentState; //{ get; private set; } Uncomment later to ensure that this is only ever changed in this script

    private PlayerOrientation bodyManager;
    private LegHandler legManager;

    // Start is called before the first frame update
    void Start()
    {
        bodyManager = GetComponentInChildren<PlayerOrientation>();
        legManager = GetComponentInChildren<LegHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInfo();
        GetEnterInput();
        GetExitInput();
        ResolveState();
    }

    private void GetInfo()
    {
        isGrounded = bodyManager.IsGrounded(out onGroundGround);
        wallDetected = bodyManager.WallGrounded();
    }

    private void GetEnterInput()
    {
        if (isFalling || isJumping)
            return;

        if(Input.GetKeyDown(KeyCode.LeftControl) && !onGroundGround)
        {
            isDescending = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isDescending))
        {
            isJumping = true;
            isDescending = false;
            jumpBuffer = 0.25f;
            bodyManager.Jump();
        }
    }

    private void GetExitInput()
    {
        if (isDescending && !Input.GetKey(KeyCode.LeftControl) || onGroundGround)
        {
            if (isGrounded)
                isDescending = false;
        }

        if(isJumping || isFalling)
        {
            if (jumpBuffer < 0 && ( wallDetected || isGrounded))
            {
                legManager.ForceMoveAllLegs();
                bodyManager.Land();

                jumpBuffer = 0.25f;

                isJumping = false;
                isFalling = false;
            }

            jumpBuffer -= Time.deltaTime;
            if (!isFalling && jumpBuffer < -1f)
                isFalling = true;
        }
    }

    private void ResolveState()
    {
        if (isFalling)
            currentState = MovementState.Falling;
        else if (isJumping)
            currentState = MovementState.Jumping;
        else if (isDescending)
            currentState = MovementState.Descending;
        else
            currentState = MovementState.Default;
    }
}
