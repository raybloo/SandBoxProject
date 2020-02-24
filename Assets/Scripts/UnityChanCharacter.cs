using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityChanCharacter : Character
{
    //[Header("Inspector header")]
    //[Tooltip("Variable tooltip in the inspector")]
    public FPCameraManager camera;

    override public void Act(bool[] actions)
    {
        
    }

    public void ManageActions(bool[] active)
    {
        //Movements
        if (active[(int)Action.moveForward] || active[(int)Action.moveBackward] || active[(int)Action.moveLeft] || active[(int)Action.moveRight])
        {
            playerSuperviser.moving = true;
            if (active[(int)Action.moveForward] && !active[(int)Action.moveBackward])
            {
                if (active[(int)Action.moveLeft] && !active[(int)Action.moveRight])
                {
                    playerSuperviser.moveVector = new Vector3(-0.70710678118f * Time.deltaTime * playerSuperviser.speed, 0f, 0.70710678118f * Time.deltaTime * playerSuperviser.speed);
                }
                else if (active[(int)Action.moveRight] && !active[(int)Action.moveLeft])
                {
                    playerSuperviser.moveVector = new Vector3(0.70710678118f * Time.deltaTime * playerSuperviser.speed, 0f, 0.70710678118f * Time.deltaTime * playerSuperviser.speed);
                }
                else
                {
                    playerSuperviser.moveVector = new Vector3(0f, 0f, Time.deltaTime * playerSuperviser.speed);
                }
            }
            else if (active[(int)Action.moveBackward] && !active[(int)Action.moveForward])
            {
                if (active[(int)Action.moveLeft] && !active[(int)Action.moveRight])
                {
                    playerSuperviser.moveVector = new Vector3(-0.70710678118f * Time.deltaTime * playerSuperviser.speed, 0f, -0.70710678118f * Time.deltaTime * playerSuperviser.speed);
                }
                else if (active[(int)Action.moveRight] && !active[(int)Action.moveLeft])
                {
                    playerSuperviser.moveVector = new Vector3(0.70710678118f * Time.deltaTime * playerSuperviser.speed, 0f, -0.70710678118f * Time.deltaTime * playerSuperviser.speed);
                }
                else
                {
                    playerSuperviser.moveVector = new Vector3(0f, 0f, -Time.deltaTime * playerSuperviser.speed);
                }
            }
            else
            {
                if (active[(int)Action.moveLeft] && !active[(int)Action.moveRight])
                {
                    playerSuperviser.moveVector = new Vector3(-Time.deltaTime * playerSuperviser.speed, 0f, 0f);
                }
                else if (active[(int)Action.moveRight] && !active[(int)Action.moveLeft])
                {
                    playerSuperviser.moveVector = new Vector3(Time.deltaTime * playerSuperviser.speed, 0f, 0f);
                }
                else
                {
                    playerSuperviser.moving = false;
                    playerSuperviser.moveVector = new Vector3(0f, 0f, 0f);
                }
            }

        }
        else
        {
            playerSuperviser.moving = false;
            playerSuperviser.moveVector = new Vector3(0f, 0f, 0f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
