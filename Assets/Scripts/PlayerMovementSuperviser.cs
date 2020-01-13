using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementSuperviser: MonoBehaviour
{
    public bool moving = false;
    

    private Vector3 facing = new Vector3(0f, 0f, 0f);
    private float walkSpeed = 5f;
    private Vector3 walkMovement = new Vector3(0f, 0f, 0f);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ManageInput(bool forward, bool backward, bool left, bool right)
    {
        if (forward || backward || left || right)
        {
            moving = true;
            if (forward && !backward)
            {
                if (left && !right)
                {
                    walkMovement = new Vector3(-0.70710678118f * Time.deltaTime * walkSpeed, 0f, 0.70710678118f * Time.deltaTime * walkSpeed);
                }
                else if (right && !left)
                {
                    walkMovement = new Vector3(0.70710678118f * Time.deltaTime * walkSpeed, 0f, 0.70710678118f * Time.deltaTime * walkSpeed);
                }
                else
                {
                    walkMovement = new Vector3(0f, 0f, Time.deltaTime * walkSpeed);
                }
            }
            else if (backward && !forward)
            {
                if (left && !right)
                {
                    walkMovement = new Vector3(-0.70710678118f * Time.deltaTime * walkSpeed, 0f, -0.70710678118f * Time.deltaTime * walkSpeed);
                }
                else if (right && !left)
                {
                    walkMovement = new Vector3(0.70710678118f * Time.deltaTime * walkSpeed, 0f, -0.70710678118f * Time.deltaTime * walkSpeed);
                }
                else
                {
                    walkMovement = new Vector3(0f, 0f, -Time.deltaTime * walkSpeed);
                }
            }
            else
            {
                if (left && !right)
                {
                    walkMovement = new Vector3(-Time.deltaTime * walkSpeed, 0f, 0f);
                }
                else if (right && !left)
                {
                    walkMovement = new Vector3(Time.deltaTime * walkSpeed, 0f, 0f);
                }
                else
                {
                    moving = false;
                    walkMovement = new Vector3(0f, 0f, 0f);
                }
            }

        }
        else
        {
            moving = false;
            walkMovement = new Vector3(0f, 0f, 0f);
        }
    }
}
