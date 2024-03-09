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

    public MovementState currentState; //{ get; private set; }
    public bool isGrounded; //{ get; private set; }
    public bool onGroundGround; //{ get; private set; }
    public bool wallDetected; //{ get; private set; }

    private bool isDescending;
    private bool isJumping;
    private bool isFalling;

    private float jumpBuffer;

    public bool lockInputForces;

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
        bool groundyGround;
        isGrounded = bodyManager.IsGrounded(out groundyGround);
        wallDetected = bodyManager.WallGrounded();
        onGroundGround = groundyGround;
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
            jumpBuffer = 0.1f;
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

                jumpBuffer = 0.1f;

                isJumping = false;
                isFalling = false;
            }

            jumpBuffer -= Time.deltaTime;
            if (!isFalling && jumpBuffer < -2f)
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
