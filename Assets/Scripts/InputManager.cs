using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class InputManager : MonoBehaviour
{
    public static readonly int actionsLength = 30;
    
    public enum Action
    {
        unused,
        moveForward,
        moveBackward,
        moveLeft,
        moveRight
    }

    public PlayerSuperviser playerSuperviser;
    public KeyboardManager keyboardManager;

    private Action[] exploreActionSet = new Action[] { Action.moveForward, Action.moveBackward, Action.moveLeft, Action.moveRight };

    // Start is called before the first frame update
    void Start()
    {
        keyboardManager.SetNewActionSet(new Action[0],exploreActionSet);
    }

    public void ManageActions(bool[] active)
    {
        Debug.Log("Move fwd: " + active[(int)Action.moveForward]);
        Debug.Log("Move bwd: " + active[(int)Action.moveBackward]);
        Debug.Log("Move left: " + active[(int)Action.moveLeft]);
        Debug.Log("Move right: " + active[(int)Action.moveRight]);
        if (active[(int)Action.moveForward] || active[(int)Action.moveBackward] || active[(int)Action.moveLeft] || active[(int)Action.moveRight])
        {
            playerSuperviser.moving = true;
            if (active[(int)Action.moveForward] && !active[(int)Action.moveBackward])
            {
                if (active[(int)Action.moveLeft] && !active[(int)Action.moveRight])
                {
                    playerSuperviser.moveVector = new Vector3(-0.70710678118f * Time.deltaTime * playerSuperviser.speed, 0f , 0.70710678118f * Time.deltaTime * playerSuperviser.speed);
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

    // Update is called once per frame
    void Update()
    {

    }
}
