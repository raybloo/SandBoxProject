using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCharacter : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public MouseCameraManager characterCam;
    public Animator characterAnimator;
    public CharacterController characterController;
    public CharacterCollisionManager collisionManager;

    private static readonly float HALF_SQRT2 = 0.70710678118f;

    //Movement and Rotation
    public MoveState moveState = MoveState.ground;
    private Vector3 front = Vector3.forward;
    //private Vector3 lookVector = Vector3.zero;
    private Vector3 right = Vector3.right;
    private float verticalSpeed = 0f;
    private float horizontalSpeed = 0f;
    private Quaternion cameraRotation = Quaternion.identity;
    private Quaternion gravityRotation = Quaternion.identity;
    private Quaternion combinedRotation = Quaternion.identity;

    //Self-Induced
    private bool moving = false;
    private Vector3 horizontalMovement = Vector3.zero;
    private Vector3 verticalMovement = Vector3.zero;
    private Vector3 direction = Vector3.forward;
    float prevTheta, prevPhi = 0;

    //Ground
    public float currRunSpeed = 0f;
    public float maxRunSpeed = 10f;
    private float groundAccel = 5f; //acceleration rate proportional to max speed
    private float groundDecel = 9f; //decelaration rate proportional to max speed
    private float turnPenalty = 0.75f;
    private float sideRunPenalty = 1.0f;
    private float backRunPenalty = 0.8f;
    private float groundCheckSpeed = 20f;

    //Jump/Airborne
    public Vector3 gravityDir = new Vector3(0, -1, 0);
    private float gravity = 30f;
    private float jumpImpulse = 12f;
    private float maxFallSpeed = Mathf.Infinity;

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

    //Act is called upon fixedUpdate
    override public void Act(bool[] action, float[] axis)
    {
        //Camera
        (float theta, float phi) = characterCam.MoveCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.fixedDeltaTime, axis[(int)Character.Axis.cameraY] * Time.fixedDeltaTime));
        characterCam.GetLookVector(out front);
        InPlaned(ref front);

        //Rotation
        CameraRotation(theta, phi, out cameraRotation);
        GravityRotation(gravityDir, out gravityRotation);
        combinedRotation = cameraRotation * gravityRotation;
        transform.localRotation = collisionManager.Rotate(in combinedRotation);
        if(collisionManager.collided) {
            characterCam.RevertCamera();
        }

        //transform.localRotation = collisionManager.Rotate(cameraRotation * gravityRotation);

        front = transform.localRotation * Vector3.forward;

        //Movement state multiplexer
        /*if ((moveState == MoveState.fly || moveState == MoveState.jump) && ) //landing
        {
            //animator.Play("Land");
            verticalSpeed = 0f;
            moveState = MoveState.ground;
            Debug.Log("Land");
        }*/
        if(action[(int)Character.Action.jump]) //jump
        {
            verticalSpeed = jumpImpulse;
            moveState = MoveState.jump;
            Debug.Log("Jump");
        }
        
        


        //Movement
        //Full control (Move Input Check)
        if (moveState != MoveState.knock)
        {
            //Check for player input
            moving = action[(int)Action.moveForward] || action[(int)Action.moveBackward] || action[(int)Action.moveLeft] || action[(int)Action.moveRight];

            //Ground (full directional control)
            if (moveState == MoveState.ground)
            {
                if(moving)
                {
                    //front.y = 0;
                    //front.Normalize();
                    if (action[(int)Action.moveLeft] ^ action[(int)Action.moveRight]) //is there lateral movement?
                    {
                        characterCam.GetLateralVector(out right);
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { //diagonal movement
                            if (action[(int)Action.moveForward])
                            {
                                if (action[(int)Action.moveRight])
                                { // forward-right
                                    horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * HALF_SQRT2) * (front + right);
                                }
                                else
                                { // forward-left
                                    horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * HALF_SQRT2) * (front - right);
                                }
                            }
                            else
                            {
                                if (action[(int)Action.moveRight])
                                { // backward-right
                                    horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front + right);
                                }
                                else
                                { // backward-left
                                    horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front - right);
                                }
                            }
                        }
                        else // only lateral movement
                        {
                            if (action[(int)Action.moveRight])
                            { // right
                                horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * sideRunPenalty) * right;
                            }
                            else
                            { // left
                                horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * sideRunPenalty) * -right;
                            }
                        }
                    }
                    else // no lateral movement
                    {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { // only forward or backward
                            if (action[(int)Action.moveForward])
                            { // forward
                                horizontalMovement = (Time.fixedDeltaTime * currRunSpeed) * front;
                            }
                            else
                            { // backward
                                horizontalMovement = (Time.fixedDeltaTime * currRunSpeed * backRunPenalty) * -front;
                            }
                        }
                        else // no movement at all
                        {
                            horizontalMovement = Vector3.zero;
                            moving = false;
                        }
                    }
                }
                else // no movement at all
                {
                    horizontalMovement = Vector3.zero;
                    moving = false;
                }


                //acceleration and deceleration
                // turn penalty model
                if(moving)
                {
                    if (horizontalMovement.magnitude > 0.001f)
                    {
                        float angle = Vector3.Angle(horizontalMovement, direction);
                        currRunSpeed = Mathf.Max(0f, currRunSpeed * (1f - (turnPenalty * angle / 180f)));
                        direction = horizontalMovement.normalized;
                    }
                    currRunSpeed = Mathf.Min(maxRunSpeed, currRunSpeed + groundAccel * maxRunSpeed * Time.fixedDeltaTime);
                } 
                else
                {
                    direction = front;
                    currRunSpeed = Mathf.Max(0, currRunSpeed - groundDecel * maxRunSpeed * Time.fixedDeltaTime);
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

        //Gravity Influence
        if(moveState == MoveState.jump || moveState == MoveState.knock)
        {
            verticalSpeed -= gravity * Time.fixedDeltaTime;
        }
        else if(moveState == MoveState.ground) 
        {
            verticalSpeed = -groundCheckSpeed;
        }
        else
        {
            verticalSpeed = 0f;
        }

        //Horizontal Movement
        if (horizontalMovement != Vector3.zero) {
            collisionManager.MoveS(in horizontalMovement,out horizontalMovement);
            transform.position += horizontalMovement;
        }

        //Vertical Movement
        if(verticalSpeed != 0f) {
            verticalMovement = Vector3.up * verticalSpeed * Time.fixedDeltaTime;
            if (moveState == MoveState.jump || moveState == MoveState.knock) 
            {
                collisionManager.MoveS(in verticalMovement, out verticalMovement);
                if(collisionManager.collided) {
                    moveState = MoveState.ground;
                }
                transform.position += verticalMovement;
            } 
            else if (moveState == MoveState.ground)
            {
                collisionManager.MoveRB(in verticalMovement, out verticalMovement);
                if (collisionManager.collided) 
                {
                    transform.position += verticalMovement;
                } 
                else 
                {
                    moveState = MoveState.jump;
                    verticalSpeed = 0f;
                }
            } 
            else 
            {

            }

        }
        
    }

    private void CameraRotation(float theta, float phi, out Quaternion cameraRotation) { //Rotation absolue
        cameraRotation = Quaternion.Euler(theta, phi, 0f);
    }

    private void GravityRotation(in Vector3 gravity, out Quaternion gravityRotation) { //Rotation ?
        gravityRotation = Quaternion.FromToRotation(Vector3.down, gravity);
    }

    private void InPlaned(ref Vector3 vec) {
        vec.y = 0;
        vec = vec.normalized;
    }
}
