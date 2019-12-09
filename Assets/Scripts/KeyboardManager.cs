using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This class translates keyboards or controller input into an
 * action managed by the InputManager
 */
public class KeyboardManager : MonoBehaviour
{
    public InputManager inputManager;
    //KeyCode enum size is approx 512
    private InputManager.Action[] currentActionSet = new InputManager.Action[0];
    private InputManager.Action[] currentActionSetLP = new InputManager.Action[0]; //LP stands for long press
    private List<KeyCode>[] keyBindings = new List<KeyCode>[InputManager.actionsLength];
    private bool[] activeActions = new bool[InputManager.actionsLength];

    void Start()
    {
        for(int i = 0; i < activeActions.Length; i++)
        {
            activeActions[i] = false;
        }
        SetupBasicKeyBinding();
    }

    public void SetNewActionSet(InputManager.Action[] newActionSet, InputManager.Action[] newActionSetLP)
    {
        for (int i = 0; i < activeActions.Length; i++) //disable action outside of the action set
        {
            activeActions[i] = false;
        }
        currentActionSet = newActionSet;
        currentActionSetLP = newActionSetLP;
    }

    void Update()
    {
        for(int i = 0; i < currentActionSet.Length; i++) // Action i
        {
            activeActions[(int)currentActionSet[i]] = false;
            if (keyBindings[(int)currentActionSet[i]].Count > 0)
            {
                foreach (KeyCode keyCode in keyBindings[(int)currentActionSet[i]])
                {
                    if(Input.GetKeyDown(keyCode))
                    {
                        activeActions[(int)currentActionSet[i]] = true;
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < currentActionSetLP.Length; i++) // Action LP i
        {
            activeActions[(int)currentActionSetLP[i]] = false;
            if (keyBindings[(int)currentActionSetLP[i]].Count > 0)
            {
                foreach (KeyCode keyCode in keyBindings[(int)currentActionSetLP[i]])
                {
                    if (Input.GetKey(keyCode))
                    {
                        activeActions[(int)currentActionSetLP[i]] = true;
                        break;
                    }
                }
            }
        }
        inputManager.ManageActions(activeActions);
    }

    private void SetupBasicKeyBinding()
    {
        keyBindings[(int)InputManager.Action.moveForward] = new List<KeyCode>() { KeyCode.W };
        keyBindings[(int)InputManager.Action.moveBackward] = new List<KeyCode>() { KeyCode.S };
        keyBindings[(int)InputManager.Action.moveLeft] = new List<KeyCode>() { KeyCode.A };
        keyBindings[(int)InputManager.Action.moveRight] = new List<KeyCode>() { KeyCode.D };
    }
}
