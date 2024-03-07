using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderState : MonoBehaviour
{
    public enum MovementState
    {
        Default,
        Jumping,
        Descending
    }

    private bool isDescending;
    private bool isJumping;

    public MovementState currentState; //{ get; private set; } Uncomment later to ensure that this is only ever changed in this script

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        ResolveState();
    }

    private void ResolveState()
    {
        if (isDescending)
            currentState = MovementState.Descending;
        else
            currentState = MovementState.Default;
    }

    private void GetInput()
    {
        isDescending = Input.GetKey(KeyCode.LeftControl);
    }
}
