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
    private MoveState moveState = MoveState.grounded;
    private bool moving = false;
    private Vector3 moveVector = new Vector3(0f,0f,0f);
    private float currRunSpeed = 10f;
    private float maxRunSpeed = 10f;
    private float runAccel = 0.2f;

    private void Start()
    {
        currRunSpeed = maxRunSpeed;
    }

    override public void Act(bool[] active, float[] axis)
    {
        //Camera
        characterCam.MoveCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));
        //Movements
        if (active[(int)Action.moveForward] || active[(int)Action.moveBackward] || active[(int)Action.moveLeft] || active[(int)Action.moveRight])
        {
            moving = true;
            Vector3 front = characterCam.GetLookVector();
            if (moveState == MoveState.grounded)
            {
                front.y = 0;
                front.Normalize();
            }

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
                            moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2) * (-front + right);
                        }
                        else
                        { // backward-left
                            moveVector = (Time.deltaTime * currRunSpeed * HALF_SQRT2) * (-front - right);
                        }
                    }
                }
                else // only lateral movement
                {
                    if (active[(int)Action.moveRight])
                    { // right
                        moveVector = (Time.deltaTime * currRunSpeed) * right;
                    }
                    else
                    { // left
                        moveVector = (Time.deltaTime * currRunSpeed) * -right;
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
                        moveVector = (Time.deltaTime * currRunSpeed) * -front;
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
        transform.position += moveVector;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
