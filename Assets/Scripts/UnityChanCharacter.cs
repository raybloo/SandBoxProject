﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCharacter : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public MouseCameraManager characterCam;
    public CharacterController characterController;
    public CharacterCollisionManager collisionManager;

    private static readonly float HALF_SQRT2 = 0.70710678118f;
    private static readonly float TWO_PI = 6.28318530718f;
    private static readonly float RAD_2_DEG = 57.2957795131f;
    private static readonly float DEG_2_RAD = 0.01745329251f;

    //Movement
    //private bool moving = false;
    public MoveState moveState = MoveState.ground;
    public Vector3 currentPosition;

    private Vector3 inputMovement = Vector3.zero; // current input direction with penalty
    private Vector3 horizontalMomentum = Vector3.zero; // previous direction and speed
    public float horizontalSpeed = 0f; // current and previous speed
    private Vector3 horizontalMovement = Vector3.zero; // current applied speed and direction
    
    private Vector3 verticalMomentum = Vector3.zero; // previous direction and speed
    public float verticalSpeed = 0f; // current and previous speed
    private Vector3 verticalMovement = Vector3.zero; // current applied direction and speed

    //Rotation
    private Quaternion cameraRotation = Quaternion.identity;
    private Quaternion gravityRotation = Quaternion.identity;
    private Quaternion combinedRotation = Quaternion.identity;
    private Vector3 front = Vector3.forward;
    private Vector3 right = Vector3.right;
    private Vector3 up = Vector3.up;
    private float theta = 0, phi = 0;
    private bool freeCamera = false;


    //Ground
    public float maxSpeed = 10f;
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
    //public float airDrag = 0.4f;
    private float jumpImpulse = 20f;
    private float maxFallSpeed = Mathf.Infinity;

    //Externally-induced
    //Knock
    private Vector3 knockVector = new Vector3(0f, 0f, 0f);

    //Movement stats
    public float speedCapAutoGenerationRate = 20f; //speedCap per second
    public float speedCapAutoGenerationThreshold = 250f;
    public float speedCapTurnToleranceRate = 450f; //degrees per second
    public float speedCapBrakeDiminutionRate = 20f; //speedCap per second
    public float strafingForwardCoeff = 0.8f;
    public float strafingLateralCoeff = 0.6f;
    public float airborneControlReduction = 0.8f;

    //Attack stats
    public GameObject projectilePrefab;
    public float attackCooldown = 0.5f;
    public float attackCurrentCd = 0f;
    public float attackExplosionForce = 0.8f;
    public float attackProjectileSpeed = 50f;
    public float attackDamage = 10f;


    //DEBUG
    public Transform cube;
    public bool[] action;
    public float[] axis;


    private void Start()
    {
        horizontalSpeed = 0f;
        if(GetComponent<CharacterController>())
        {
            characterController = GetComponent<CharacterController>();
        }
    }

    private void Update() {
        //Act(action, axis);
    }

    //Act is called upon fixedUpdate
    override public void Act(bool[] action, float[] axis)
    {
        //Camera
        if(action[(int)Character.Action.slide]) {
            freeCamera = true;
            characterCam.MoveFreeCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));
        //} else if(action[(int)Character.Action.ability1]){
        //    (theta, phi) = characterCam.MoveCharacterCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));
        } else {
            if(freeCamera) { // Revert the camera when going from the free state to the horizontal control
                characterCam.RevertCamera();
                freeCamera = false;
            }
            phi = characterCam.MoveHorizontalCamera(new Vector2(axis[(int)Character.Axis.cameraX] * Time.deltaTime, axis[(int)Character.Axis.cameraY] * Time.deltaTime));
        }


        //Rotation
        currentPosition = transform.position;
        SelectGravityManager(currentPosition);
        if (currentGravityManager) {
            currentGravityManager.GetGravityDir(currentPosition, ref gravityTarget);
            gravityDir = Vector3.RotateTowards(gravityDir, gravityTarget, gravityRotationSpeed * DEG_2_RAD * Time.deltaTime, 0);
        }
        else {
            //TODO if the gravity manager is properly implemented, this code can be removed
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
        up = transform.up;

        //Momentum dispatch 
        /* physically somewhat more correct yet behaving weirdly especially if the gravity has a big variability
        verticalSpeed = (Vector3.Dot(horizontalMomentum, gravityDir) + Vector3.Dot(verticalMomentum, gravityDir));         
        horizontalMomentum = horizontalMomentum + verticalMomentum - (gravityDir * verticalSpeed);
        verticalMomentum = gravityDir * verticalSpeed;
        horizontalSpeed = horizontalMomentum.magnitude;
        */


        horizontalMomentum = (horizontalMomentum - gravityDir * Vector3.Dot(horizontalMomentum, gravityDir)).normalized * horizontalSpeed;
        verticalMomentum = gravityDir * verticalSpeed;
       // Debug.DrawLine(currentPosition, currentPosition + horizontalMomentum);

        //Apply Air drag if Airborne
        /*if (moveState == MoveState.jump || moveState == MoveState.knock) {
            float airDragCoeff = 1 - (airDrag * Time.deltaTime);
            horizontalMomentum *= airDragCoeff;
            verticalMomentum *= airDragCoeff;
            horizontalSpeed *= airDragCoeff;
            verticalSpeed *= airDragCoeff;
        }*/

        //Gravity Influence
        if (moveState == MoveState.jump || moveState == MoveState.knock) {
            verticalSpeed += gravity * Time.deltaTime;
            verticalMomentum = gravityDir * verticalSpeed;
        } else {
            verticalSpeed = 0f;
            verticalMomentum = Vector3.zero;
        }

        //Regular Attack
        if (action[(int)Character.Action.attack]) { 
            if(Time.time > attackCurrentCd) {
                Projectile projectile = Instantiate(projectilePrefab, characterCam.transform.position, characterCam.transform.rotation).GetComponent<Projectile>();
                projectile.explosionForce = speedCap * attackExplosionForce;
                projectile.speed = maxSpeed + attackProjectileSpeed;
                attackCurrentCd = Time.time + attackCooldown;
                //TODO animation
                //characterAnimator.SetBool("airborne", true);
                //characterAnimator.SetBool("rising", true);
            }

        }

        //Jump
        if (action[(int)Character.Action.jump]) { //&& moveState == MoveState.ground) {
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
            //moving = !action[(int)Action.brake];//action[(int)Action.moveForward] || action[(int)Action.moveBackward] || action[(int)Action.moveLeft] || action[(int)Action.moveRight];

            //Ground (full directional control)
            if (moveState == MoveState.ground)
            {
                //Determine Direction
                if(action[(int)Action.strafeLeft] ^ action[(int)Action.strafeRight])
                {
                    if (action[(int)Action.strafeLeft]) {
                        inputMovement = front * strafingForwardCoeff - right * strafingLateralCoeff;
                    } 
                    else 
                    {
                        inputMovement = front * strafingForwardCoeff + right * strafingLateralCoeff;
                    }
                }
                else // no movement at all
                {
                    inputMovement = front;
                }


                //Determine Speed and Direction
                float angle = Vector3.Angle(horizontalMomentum, inputMovement);
                if (angle > speedCapTurnToleranceRate * Time.deltaTime) {
                    speedCap *= 0.5f + ((angle - (speedCapTurnToleranceRate * Time.deltaTime)) / 360f);
                }
                if (!action[(int)Action.brake]) //Acceleration
                {
                    if (speedCap < speedCapAutoGenerationThreshold) {
                        speedCap = Mathf.Min(speedCap + Time.deltaTime * speedCapAutoGenerationRate, speedCapAutoGenerationThreshold);
                    }
                    maxSpeed = Mathf.Sqrt(speedCap * 10f);
                    horizontalMomentum = (inputMovement * maxSpeed);
                    horizontalSpeed = horizontalMomentum.magnitude;
                } else //Braking
                  {
                    if (speedCap < speedCapAutoGenerationThreshold) {
                        speedCap = Mathf.Max(speedCap - Time.deltaTime * speedCapAutoGenerationRate, 0f);
                    }
                    maxSpeed = Mathf.Sqrt(speedCap * 5f);
                    horizontalMomentum = (inputMovement * maxSpeed);
                    horizontalSpeed = horizontalMomentum.magnitude;
                }
            }

            //Jumping (reduced directional control)
            if (moveState == MoveState.jump)
            {
                //Determine Direction
                if (!action[(int)Action.brake]) {
                    inputMovement = front;
                } 
                else // no movement at all
                {

                }


                //Determine Speed and Direction
                float angle = Vector3.Angle(horizontalMomentum, inputMovement);
                if (angle > speedCapTurnToleranceRate * Time.deltaTime) {
                    speedCap *= 0.5f + ((angle - (speedCapTurnToleranceRate * Time.deltaTime)) / 360f);
                }
                if (!action[(int)Action.brake]) //Acceleration
                {
                    if (speedCap < speedCapAutoGenerationThreshold) {
                        speedCap = Mathf.Min(speedCap + Time.deltaTime * speedCapAutoGenerationRate, speedCapAutoGenerationThreshold);
                    }
                    maxSpeed = Mathf.Sqrt(speedCap * 10f);
                    horizontalMomentum = (inputMovement * maxSpeed);
                    horizontalSpeed = horizontalMomentum.magnitude;
                } else //Braking
                  {
                    if (speedCap < speedCapAutoGenerationThreshold) {
                        speedCap = Mathf.Max(speedCap - Time.deltaTime * speedCapAutoGenerationRate, 0f);
                    }
                    maxSpeed = Mathf.Sqrt(speedCap * 5f);
                    horizontalMomentum = (inputMovement * maxSpeed);
                    horizontalSpeed = horizontalMomentum.magnitude;
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
            horizontalMovement = Time.deltaTime * horizontalMomentum;
            if(!collisionManager.MoveFC(in horizontalMovement, out horizontalMovement)) {
                horizontalMovement -= gravityDir * Time.deltaTime;
                if (!collisionManager.MoveFC(in horizontalMovement, out horizontalMovement)) {
                    horizontalMovement -= gravityDir * Time.deltaTime;
                    if (!collisionManager.MoveFC(in horizontalMovement, out horizontalMovement)) {
                        horizontalMovement -= gravityDir * Time.deltaTime;
                        if (!collisionManager.MoveFC(in horizontalMovement, out horizontalMovement)) {
                            horizontalMovement -= gravityDir * Time.deltaTime;
                            collisionManager.MoveFC(in horizontalMovement, out horizontalMovement);
                        }
                    }
                }
            }
            if(Vector3.Angle(planProject(horizontalMovement, transform.up), inputMovement) > 30f) {
                Debug.LogWarning("Hard Collision " + Vector3.Angle(planProject(horizontalMovement, transform.up), inputMovement));
                //Debug.LogWarning("Input " + inputMovement);
                //Debug.LogWarning("Movement " + planProject(horizontalMovement, transform.up));
            }
            transform.position += horizontalMovement;
        }
        
        //Apply Vertical Movement
        verticalMovement = verticalMomentum * Time.deltaTime;
        if (moveState == MoveState.jump || moveState == MoveState.knock) 
        {
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
            verticalMovement = gravityDir * (Time.deltaTime * (traction + (slopeMax * horizontalSpeed)));
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
                verticalSpeed = traction / 2;
                verticalMomentum = gravityDir * verticalSpeed;
            }
        } 
        else 
        {

        }

        //Abilities
        if (freeCamera) {

        }

        //Animations
        characterAnimator.SetBool("moving", horizontalSpeed > 0f);
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

    //Assumes n is normalized
    private Vector3 planProject(in Vector3 u, in Vector3 n) {
        return u - (Vector3.Dot(u, n) * n);
    }

    /*
    private void InPlan(ref Vector3 vec, in Vector3 plan) {
        vec -= plan * Vector3.Dot(vec, plan) / plan.magnitude;
    }
    */
}
