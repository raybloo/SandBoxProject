using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCharacter1 : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public MouseCameraManager characterCam;
    public Animator characterAnimator;
    public CharacterController characterController;
    public CharacterCollisionManager collisionManager;

    private static readonly float HALF_SQRT2 = 0.70710678118f;
    private static readonly float TWO_PI = 6.28318530718f;
    private static readonly float RAD_2_DEG = 57.2957795131f;
    private static readonly float DEG_2_RAD = 0.01745329251f;

    //Movement
    private bool moving = false;
    public MoveState moveState = MoveState.ground;
    public Vector3 currentPosition;

    private Vector3 inputMovement = Vector3.zero; // current input direction with penalty
    private Vector3 horizontalMomentum = Vector3.zero; // previous direction and speed
    public float horizontalSpeed = 0f; // current and previous speed
    private Vector3 horizontalMovement = Vector3.zero; // current applied speed and direction
    
    private Vector3 verticalMomentum = Vector3.zero; // previous direction and speed
    public float verticalSpeed = 0f; // current and previous speed
    private Vector3 verticalMovement = Vector3.zero; // current applied direction and speed

    Vector3 accelDir = Vector3.zero;

    //Rotation
    private Quaternion cameraRotation = Quaternion.identity;
    private Quaternion gravityRotation = Quaternion.identity;
    private Quaternion combinedRotation = Quaternion.identity;
    private Vector3 front = Vector3.forward;
    private Vector3 right = Vector3.right;


    //Ground
    public float maxSpeed = 10f;
    public float groundAccel = 2f; //acceleration rate
    public float groundDecel = 9f; //decelaration rate
    private float sideRunPenalty = 1.0f;
    private float backRunPenalty = 0.8f;
    public float slopeMax = 3f; //slope
    public float traction = 1f; //constant speed toward the ground
    public float animationSpeedRatio = 0.5f;

    //Jump/Airborne
    public Vector3 gravityDir = new Vector3(0, -1, 0); //Direction of the current gravity, normalized
    public Vector3 gravityTarget = new Vector3(0, -1, 0); //Always in the direction of the center, 
    public Vector3 gravityCenter = new Vector3(0, 10, 0);
    public float gravityRotationSpeed = 90f; //Gravity vector changes are not instantaneous
    public float gravity = 30f;
    public bool gravityAttracts = true;
    public float airAccel;
    public float airDecel;
    public float airDrag = 0.4f;
    private float jumpImpulse = 20f;
    private float maxFallSpeed = Mathf.Infinity;

    //Externally-induced
    //Knock
    private Vector3 knockVector = new Vector3(0f, 0f, 0f);

    //DEBUG
    public Transform cube;

    private void Start()
    {
        horizontalSpeed = 0f;
        if(GetComponent<CharacterController>())
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    //Act is called upon fixedUpdate
    override public void Act(bool[] action, float[] axis)
    {
        //Camera
        float theta = 0;
        float phi = characterCam.MoveHorizontalCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.fixedDeltaTime, axis[(int)Character.Axis.cameraY] * Time.fixedDeltaTime));

        //Rotation
        currentPosition = transform.position;
        SelectGravityManager(currentPosition);
        if (currentGravityManager) {
            currentGravityManager.GetGravityDir(currentPosition, ref gravityTarget);
            gravityDir = Vector3.RotateTowards(gravityDir, gravityTarget, gravityRotationSpeed * DEG_2_RAD * Time.fixedDeltaTime, 0);
        }
        else {
            if (gravityAttracts) {
                gravityDir = (gravityCenter - currentPosition).normalized;
            } else {
                gravityDir = (currentPosition - gravityCenter).normalized;
            }
        }
        CameraRotation(theta, phi, out cameraRotation); //Relative to previous rotation
        GravityRotation(in gravityDir, ref front, out gravityRotation); //Relative for the forward vector, absolute for the gravity vector
        combinedRotation = gravityRotation * cameraRotation;
        //transform.localRotation = collisionManager.Rotate(in combinedRotation);
        transform.localRotation = combinedRotation;
        front = transform.forward;
        right = transform.right;

        //Momentum dispatch 
        /* physically somewhat more correct yet behaving weirdly with a big variability in the gravity
        verticalSpeed = (Vector3.Dot(horizontalMomentum, gravityDir) + Vector3.Dot(verticalMomentum, gravityDir));         
        horizontalMomentum = horizontalMomentum + verticalMomentum - (gravityDir * verticalSpeed);
        verticalMomentum = gravityDir * verticalSpeed;
        horizontalSpeed = horizontalMomentum.magnitude;
        */
        horizontalMomentum = (horizontalMomentum - gravityDir * Vector3.Dot(horizontalMomentum, gravityDir)).normalized * horizontalSpeed;
        verticalMomentum = gravityDir * verticalSpeed;
        Debug.DrawLine(currentPosition, currentPosition + horizontalMomentum);

        //Apply Air drag if Airborne
        /*if (moveState == MoveState.jump || moveState == MoveState.knock) {
            float airDragCoeff = 1 - (airDrag * Time.fixedDeltaTime);
            horizontalMomentum *= airDragCoeff;
            verticalMomentum *= airDragCoeff;
            horizontalSpeed *= airDragCoeff;
            verticalSpeed *= airDragCoeff;
        }*/

        //Gravity Influence
        if (moveState == MoveState.jump || moveState == MoveState.knock) {
            verticalSpeed += gravity * Time.fixedDeltaTime;
            verticalMomentum = gravityDir * verticalSpeed;
        } else {
            verticalSpeed = 0f;
            verticalMomentum = Vector3.zero;
        }

        //Jump
        if (action[(int)Character.Action.jump]) {
            verticalSpeed = -jumpImpulse;
            verticalMomentum = gravityDir * verticalSpeed;
            moveState = MoveState.jump;
            characterAnimator.SetBool("airborne", true);
            characterAnimator.SetBool("rising", true);
        }

        //Movement
        //Full control (Move Input Check)
        if (moveState != MoveState.knock)
        {
            //Check for player input
            moving = action[(int)Action.moveForward] || action[(int)Action.moveBackward] || action[(int)Action.strafeLeft] || action[(int)Action.strafeRight];

            //Ground (full directional control)
            if (moveState == MoveState.ground)
            {
                //Determine Direction
                if(moving)
                {
                    if (action[(int)Action.strafeLeft] ^ action[(int)Action.strafeRight]) //is there lateral movement?
                    {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { //diagonal movement
                            if (action[(int)Action.moveForward])
                            {
                                if (action[(int)Action.strafeRight])
                                { // forward-right
                                    inputMovement = HALF_SQRT2 * (front + right);
                                }
                                else
                                { // forward-left
                                    inputMovement = HALF_SQRT2 * (front - right);
                                }
                            }
                            else
                            {
                                if (action[(int)Action.strafeRight])
                                { // backward-right
                                    inputMovement = (HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front + right);
                                }
                                else
                                { // backward-left
                                    inputMovement = (HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front - right);
                                }
                            }
                        }
                        else // only lateral movement
                        {
                            if (action[(int)Action.strafeRight])
                            { // right
                                inputMovement = sideRunPenalty * right;
                            }
                            else
                            { // left
                                inputMovement = sideRunPenalty * -right;
                            }
                        }
                    }
                    else // no lateral movement
                    {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { // only forward or backward
                            if (action[(int)Action.moveForward])
                            { // forward
                                inputMovement = front;
                            }
                            else
                            { // backward
                                inputMovement = backRunPenalty * -front;
                            }
                        }
                        else // no movement at all
                        {
                            moving = false;
                        }
                    }
                }
                else // no movement at all
                {

                }


                //Determine Speed and Direction
                if (moving) //Acceleration
                {
                    accelDir = (inputMovement * maxSpeed) - horizontalMomentum;
                    float turnAccel = groundAccel + ((groundDecel - groundAccel) * Vector3.Angle(horizontalMomentum, inputMovement) / 180f);
                    if (accelDir.magnitude < turnAccel * Time.fixedDeltaTime) {
                        horizontalMomentum = (inputMovement * maxSpeed);
                    } else {
                        horizontalMomentum += (accelDir.normalized * turnAccel * Time.fixedDeltaTime);
                    }
                    horizontalSpeed = horizontalMomentum.magnitude;
                } else //Deceleration
                  {
                    if (horizontalMomentum != Vector3.zero) {
                        horizontalSpeed = Mathf.Max(0f, horizontalSpeed - (groundDecel * Time.fixedDeltaTime));
                        horizontalMomentum = horizontalMomentum.normalized * horizontalSpeed;
                    }
                }


            }
            if(moveState == MoveState.jump)
            {
                if (moving) {
                    if (action[(int)Action.strafeLeft] ^ action[(int)Action.strafeRight]) //is there lateral movement?
                    {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { //diagonal movement
                            if (action[(int)Action.moveForward]) {
                                if (action[(int)Action.strafeRight]) { // forward-right
                                    inputMovement = HALF_SQRT2 * (front + right);
                                } else { // forward-left
                                    inputMovement = HALF_SQRT2 * (front - right);
                                }
                            } else {
                                if (action[(int)Action.strafeRight]) { // backward-right
                                    inputMovement = (HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front + right);
                                } else { // backward-left
                                    inputMovement = (HALF_SQRT2 * (sideRunPenalty + backRunPenalty) * 0.5f) * (-front - right);
                                }
                            }
                        } else // only lateral movement
                          {
                            if (action[(int)Action.strafeRight]) { // right
                                inputMovement = sideRunPenalty * right;
                            } else { // left
                                inputMovement = sideRunPenalty * -right;
                            }
                        }
                    } else // no lateral movement
                      {
                        if (action[(int)Action.moveForward] ^ action[(int)Action.moveBackward]) //is there forward or backward movement?
                        { // only forward or backward
                            if (action[(int)Action.moveForward]) { // forward
                                inputMovement = front;
                            } else { // backward
                                inputMovement = backRunPenalty * -front;
                            }
                        } else // no movement at all
                          {
                            moving = false;
                        }
                    }
                } 
                else // no movement at all
                {

                }

                //Determine Speed and Direction
                if (moving) //Acceleration
                {
                    accelDir = (inputMovement * maxSpeed) - horizontalMomentum;
                    float turnAccel = airAccel + ((airDecel - airAccel) * Vector3.Angle(horizontalMomentum, inputMovement) / 180f);
                    if (accelDir.magnitude < turnAccel * Time.fixedDeltaTime) {
                        horizontalMomentum = (inputMovement * maxSpeed);
                    } else {
                        horizontalMomentum += (accelDir.normalized * turnAccel * Time.fixedDeltaTime);
                    }
                    horizontalSpeed = horizontalMomentum.magnitude;
                } else //Deceleration
                {
                    if (horizontalMomentum != Vector3.zero) {
                        horizontalSpeed = Mathf.Max(0f, horizontalSpeed - (airDecel * Time.fixedDeltaTime));
                        horizontalMomentum = horizontalMomentum.normalized * horizontalSpeed;
                    }
                }

            }
            
        }
        //No control
        else
        {
            //horizontalSpeed = 0f;
        }

        //Apply Horizontal Movement
        if (horizontalMomentum != Vector3.zero) {
            horizontalMovement = Time.fixedDeltaTime * horizontalMomentum;
            if(!collisionManager.MoveS(in horizontalMovement, out horizontalMovement)) {
                horizontalMovement -= gravityDir * Time.deltaTime;
                if (!collisionManager.MoveS(in horizontalMovement, out horizontalMovement)) {
                    horizontalMovement -= gravityDir * Time.deltaTime;
                    if (!collisionManager.MoveS(in horizontalMovement, out horizontalMovement)) {
                        horizontalMovement -= gravityDir * Time.deltaTime;
                        collisionManager.MoveS(in horizontalMovement, out horizontalMovement);
                    }
                }
            }
            transform.position += horizontalMovement;
        }
        
        //Apply Vertical Movement
        verticalMovement = verticalMomentum * Time.fixedDeltaTime;
        if (moveState == MoveState.jump || moveState == MoveState.knock) 
        {
            Debug.Log("Hello");
            collisionManager.MoveS(in verticalMovement, out verticalMovement);
            if(collisionManager.collided && verticalSpeed > 0f) {
                moveState = MoveState.ground;
                characterAnimator.SetBool("airborne", false);
            }
            characterAnimator.SetBool("rising", Vector3.Dot(verticalMovement, gravityDir) < 0);
            transform.position += verticalMovement;
        } 
        else if (moveState == MoveState.ground)
        {
            verticalMovement = gravityDir * (Time.fixedDeltaTime * (traction + (slopeMax * horizontalSpeed)));
            collisionManager.MoveRB(in verticalMovement, out verticalMovement);
            if (collisionManager.collided) 
            {
                transform.position += verticalMovement;
            } 
            else 
            {
                moveState = MoveState.jump;
                characterAnimator.SetBool("airborne", true);
                characterAnimator.SetBool("rising", false);
                verticalSpeed = traction/2;
                verticalMomentum = gravityDir * verticalSpeed;
            }
        } 
        else 
        {

        }

        //Animations
        characterAnimator.SetBool("moving", moving);
        characterAnimator.SetFloat("speed", horizontalSpeed * animationSpeedRatio);
    }

    private void CameraRotation(float theta, float phi, out Quaternion cameraRotation) { //Rotation absolue
        cameraRotation = Quaternion.Euler(theta, phi, 0f);
    }

    private void GravityRotation(in Vector3 gravity, ref Vector3 forward, out Quaternion gravityRotation) { //Rotation ?
        forward -= -gravity * Vector3.Dot(forward, -gravity);
        gravityRotation = Quaternion.LookRotation(forward, -gravity);
        /*Quaternion n = Quaternion.FromToRotation(Vector3.down, gravity);
        if (n.w != 0 && n.w != 1f) {
            float normYW = n.w;
            float normXZ = Mathf.Sqrt(1 - (n.w * n.w));
            float angleOffset = 90f * DEG_2_RAD;
            float angle = Mathf.Acos(n.x / normXZ);
            if(n.z < 0 && n.w > 0 || n.z > 0 && n.w < 0) {
                angle = TWO_PI - angle;
                //Debug.Log("cos: " + (n.x / normXZ) + " angle: " + (angle * RAD_2_DEG) + " cos: " + Mathf.Cos(angle) + " with inversion");
            } else {
                //Debug.Log("cos: " + (n.x / normXZ) + " angle: " + (angle * RAD_2_DEG) + " cos: " + Mathf.Cos(angle));
            }
            angle += angleOffset;
            float phase = 0;
            float x = Mathf.Cos(angle/2) * normXZ;
            float y = Mathf.Cos(angle/2 + phase) * normYW;
            float z = Mathf.Sin(angle/2) * normXZ;
            float w = Mathf.Sin(angle/2 + phase) * normYW;
            
            gravityRotation = new Quaternion(x, y, z, w);
            //float w = n.w * Mathf.Sqrt(1 - (f * f));
        }
        else {
            gravityRotation = n;
        }
        */

        //gravityRotation = n;
        //gravityRotation.y = y;
        //gravityRotation.w = w;
        //gravityRotation *= Quaternion.AngleAxis(facing, gravity);
    }

    private void InPlaned(ref Vector3 vec) {
        vec.y = 0;
        vec = vec.normalized;
    }

    /*
    private void InPlan(ref Vector3 vec, in Vector3 plan) {
        vec -= plan * Vector3.Dot(vec, plan) / plan.magnitude;
    }
    */
}
