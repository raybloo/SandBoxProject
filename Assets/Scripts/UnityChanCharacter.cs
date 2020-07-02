using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCharacter : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public FPCameraManager characterCam;
    public Animator characterAnimator;

    private static readonly float HALF_SQRT2 = 0.70710678118f;

    //Movement
    private MoveState moveState = MoveState.ground;


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
    //Fly

    private float test = 0f;

    //Externally-induced
    //Knock
    private Vector3 knockVector = new Vector3(0f, 0f, 0f);

    private void Start()
    {
        currRunSpeed = 0f;
    }

    override public void Act(bool[] active, float[] axis)
    {
        //Camera
        characterCam.MoveCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));

        //Movement
        //Full control (Move Input Check)
        if (moveState != MoveState.knock)
        {
            //Check for player input
            Vector3 front = characterCam.GetLookVector();
            moving = active[(int)Action.moveForward] || active[(int)Action.moveBackward] || active[(int)Action.moveLeft] || active[(int)Action.moveRight];

            //Ground (full directional control)
            if (moveState == MoveState.ground)
            {
                if(moving)
                {
                    front.y = 0;
                    front.Normalize();
                    if (active[(int)Action.moveLeft] ^ active[(int)Action.moveRight]) //is there lateral movement?
                    {
                        Vector3 right = characterCam.GetLateralVector();
                        if (active[(int)Action.moveForward] ^ active[(int)Action.moveBackward]) //is there forward or backward movement?
                        { //diagonal movement
                            if (active[(int)Action.moveForward])
                            {
                                if (active[(int)Action.moveRight])
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
                                if (active[(int)Action.moveRight])
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
                            if (active[(int)Action.moveRight])
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
                        if (active[(int)Action.moveForward] ^ active[(int)Action.moveBackward]) //is there forward or backward movement?
                        { // only forward or backward
                            if (active[(int)Action.moveForward])
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

        transform.position += moveVector;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
