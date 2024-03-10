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

    private float biteBuffer;
    private float biteCooldown;
    private float jumpBuffer;

    public bool lockInputForces;

    private PlayerOrientation bodyManager;
    private LegHandler legManager;
    private FangControllers fangManager;

    // Start is called before the first frame update
    void Start()
    {
        bodyManager = GetComponentInChildren<PlayerOrientation>();
        legManager = GetComponentInChildren<LegHandler>();
        fangManager = GetComponentInChildren<FangControllers>();
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
        if (Input.GetMouseButtonDown(0))
            biteBuffer = 0.1f;

        if (biteBuffer > 0 && biteCooldown < 0)
        {
            fangManager.Bite();
            biteBuffer = 0;
            biteCooldown = 0.2f;
        }

        if (isFalling || isJumping)
            return;

        if(Input.GetKeyDown(KeyCode.C) && !onGroundGround)
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
        if (isDescending && !Input.GetKey(KeyCode.C) || onGroundGround)
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

        biteCooldown -= Time.deltaTime;
        biteBuffer -= Time.deltaTime;

        //if (!isFalling && !isDescending && !isJumping && !isGrounded && !wallDetected)
        //    isFalling = true;
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
