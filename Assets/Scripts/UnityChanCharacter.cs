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
    private float maxRunSpeed = 30f;
    private float groundAccel = 2f;
    private float groundDecel = 2f;
    private float sideRunPenalty = 0.9f;
    private float backRunPenalty = 0.8f;
    //Fly

    private float test = 0f;

    //Externally-induced
    //Knock
    private Vector3 knockVector = new Vector3(0f, 0f, 0f);

    private void Start()
    {
        currRunSpeed = maxRunSpeed;
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

            //Check for move type
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

                /** slide model
                 * if ((moveVector - pastMoveVector).magnitude > groundAccel * Time.deltaTime)
                 *  {
                 *    moveVector = ((moveVector - pastMoveVector) * groundAccel * Time.deltaTime) + pastMoveVector;
                 *  }
                 */

                // turn penalty model
                if(moving)
                {
                    if(moveVector.magnitude > 0.001f)
                    {
                        float turnPenalty = 0.5f + 1f * Vector3.Dot(moveVector, direction) / (-2 * moveVector.magnitude);
                        test += turnPenalty;
                        currRunSpeed = Mathf.Max(0f, currRunSpeed - (maxRunSpeed * 0.5f * turnPenalty));
                        direction = moveVector.normalized;
                    }
                    currRunSpeed = Mathf.Min(maxRunSpeed, currRunSpeed + groundAccel * Time.deltaTime);
                    Debug.Log(test);
                } 
                else
                {
                    currRunSpeed -= groundDecel * Time.deltaTime;
                }
                //Debug.Log(currRunSpeed);
            }

            
            
        }
        //No control
        else
        {

        }/*
        else // no movement at all
        {
            moveVector = Vector3.zero;
            moving = false;
        }*/
        transform.position += moveVector;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
