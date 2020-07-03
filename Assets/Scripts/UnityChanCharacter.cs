using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCharacter : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public FPCameraManager characterCam;
    public Animator characterAnimator;
    public CharacterController characterController;

    private static readonly float HALF_SQRT2 = 0.70710678118f;

    //Movement
    private MoveState moveState = MoveState.ground;
    private float verticalSpeed = 0f;
    private float horizontalSpeed = 0f;

    //Self-Induced
    private bool moving = false;
    private Vector3 moveVector = new Vector3(0f, 0f, 0f);
    private Vector3 direction = new Vector3(0f, 0f, 1f);
    //Ground
    private float currRunSpeed = 0f;
    private float maxRunSpeed = 10f;
    private float groundAccel = 5f; //acceleration rate proportional to max speed
    private float groundDecel = 9f; //decelaration rate proportional to max speed
    private float turnPenalty = 0.75f;
    private float sideRunPenalty = 1.0f;
    private float backRunPenalty = 0.8f;
    //Jump/Airborne
    private float gravity = 20f;
    private float jumpImpulse = 8f;
    private float maxFallSpeed = Mathf.NegativeInfinity;

    private float test = 0f;

    //Externally-induced
    //Knock
    private Vector3 knockVector = new Vector3(0f, 0f, 0f);

    private void Start()
    {
        currRunSpeed = 0f;
        if(GetComponent<CharacterController>())
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    override public void Act(bool[] action, float[] axis)
    {
        //Camera
        characterCam.MoveCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));

        //Movement state multiplexer
        if((moveState == MoveState.fly || moveState == MoveState.jump) && characterController.isGrounded) //landing
        {
            //animator.Play("Land");
            verticalSpeed = 0f;
            moveState = MoveState.ground;
            Debug.Log("Land");
        }
        if(moveState == MoveState.ground && action[(int)Character.Action.jump]) //jump
        {
            verticalSpeed += jumpImpulse;
            moveState = MoveState.jump;
            Debug.Log("Jump");
        }


        //Movement
        //Full control (Move Input Check)
        if (moveState != MoveState.knock)
        {
            //Check for player input
            Vector3 front = characterCam.GetLookVector();
            moving = action[(int)Action.moveForward] || action[(int)Action.moveBackward] || action[(int)Action.moveLeft] || action[(int)Action.moveRight];

            //Ground (full directional control)
            if (moveState == MoveState.ground)
            {
                if(moving)
                {
                    front.y = 0;
                    front.Normalize();
                    if (action[(int)Action.moveLeft] ^ action[(int)Action.moveRight]) //is there lateral movement?
                    {
                        Vector3 right = characterCam.GetLateralVector();
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { //diagonal movement
                            if (action[(int)Action.moveForward])
                            {
                                if (action[(int)Action.moveRight])
                                { // forward-right
                                    moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2) * (front + right);
                                }
                                else
                                { // forward-left
                                    moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2) * (front - right);
                                }
                            }
                            else
                            {
                                if (action[(int)Action.moveRight])
                                { // backward-right
                                    moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front + right);
                                }
                                else
                                { // backward-left
                                    moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front - right);
                                }
                            }
                        }
                        else // only lateral movement
                        {
                            if (action[(int)Action.moveRight])
                            { // right
                                moveVector = (Time.deltaTime * currRunSpeed * sideRunPenalty) * right;
                            }
                            else
                            { // left
                                moveVector = (Time.deltaTime * currRunSpeed * sideRunPenalty) * -right;
                            }
                        }
                    }
                    else // no lateral movement
                    {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { // only forward or backward
                            if (action[(int)Action.moveForward])
                            { // forward
                                moveVector = (Time.deltaTime * currRunSpeed) * front;
                            }
                            else
                            { // backward
                                moveVector = (Time.deltaTime * currRunSpeed * backRunPenalty) * -front;
                            }
                        }
                        else // no movement at all
                        {
                            moveVector = Vector3.zero;
                            moving = false;
                        }
                    }
                }
                else // no movement at all
                {
                    moveVector = Vector3.zero;
                    moving = false;
                }


                //acceleration and deceleration
                // turn penalty model
                if(moving)
                {
                    if (moveVector.magnitude > 0.001f)
                    {
                        float angle = Vector3.Angle(moveVector, direction);
                        currRunSpeed = Mathf.Max(0f, currRunSpeed * (1f - (turnPenalty * angle / 180f)));
                        direction = moveVector.normalized;
                    }
                    currRunSpeed = Mathf.Min(maxRunSpeed, currRunSpeed + groundAccel * maxRunSpeed * Time.deltaTime);
                } 
                else
                {
                    direction = front;
                    currRunSpeed = Mathf.Max(0, currRunSpeed - groundDecel * maxRunSpeed * Time.deltaTime);
                }
            }
            if(moveState == MoveState.jump)
            {

            }

            
            
        }
        //No control
        else
        {
            currRunSpeed = 0f;
        }

        if(moveState == MoveState.jump || moveState == MoveState.knock)
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }
        else
        {
            verticalSpeed = 0f;
        }
        moveVector.y = (verticalSpeed * Time.deltaTime);
        characterController.Move(moveVector);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
