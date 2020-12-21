using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * This class translates keyboards or controller boolean input into an
 * action managed by the ActionManager
 */
public class InputManager : MonoBehaviour
{
    public ActionManager actionManager;
    //KeyCode enum size is approx 512
    private ActionManager.Action[] currentActionSetPress = new ActionManager.Action[0]; //Press Actions are fired only once when pressing the button
    private ActionManager.Action[] currentActionSet = new ActionManager.Action[0]; //Normal Actions are fired continuously while maintaining the button down
    private ActionManager.Axis[] currentAxisSet = new ActionManager.Axis[0]; //Axis are joysticks and pointing device that use numeric values instead of boolean
    private List<KeyCode>[] keyBindings = new List<KeyCode>[ActionManager.actionsLength];
    private string[] axisBindings = new string[ActionManager.axisLength];
    private bool[] activeActions = new bool[ActionManager.actionsLength];
    private float[] axisValues = new float[ActionManager.axisLength];

   
    void Start()
    {
        for(int i = 0; i < activeActions.Length; i++)
        {
            activeActions[i] = false;
        }
        for (int i = 0; i < axisValues.Length; i++)
        {
            axisValues[i] = 0;
        }
        SetupBasicKeyBinding();
    }

    public void SetNewActionSet(ActionManager.Action[] newActionSetPress, ActionManager.Action[] newActionSet)
    {
        for (int i = 0; i < activeActions.Length; i++) //disable action outside of the action set
        {
            activeActions[i] = false;
        }
        currentActionSetPress = newActionSetPress;
        currentActionSet = newActionSet;
    }

    public void SetNewAxisSet(ActionManager.Axis[] newAxisSet)
    {
        for (int i = 0; i < axisValues.Length; i++)
        {
            axisValues[i] = 0;
        }
        currentAxisSet = newAxisSet;
    }

    void FixedUpdate()
    {
        for(int i = 0; i < currentActionSetPress.Length; i++) // Press Action i
        {
            activeActions[(int)currentActionSetPress[i]] = false;
            if (keyBindings[(int)currentActionSetPress[i]].Count > 0)
            {
                foreach (KeyCode keyCode in keyBindings[(int)currentActionSetPress[i]])
                {
                    if(Input.GetKeyDown(keyCode))
                    {
                        activeActions[(int)currentActionSetPress[i]] = true;
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < currentActionSet.Length; i++) // Normal Action i
        {
            activeActions[(int)currentActionSet[i]] = false;
            if (keyBindings[(int)currentActionSet[i]].Count > 0)
            {
                foreach (KeyCode keyCode in keyBindings[(int)currentActionSet[i]])
                {
                    if (Input.GetKey(keyCode))
                    {
                        activeActions[(int)currentActionSet[i]] = true;
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < currentAxisSet.Length; i++) // Axis i
        {
            axisValues[(int)currentAxisSet[i]] = 0f;
            if (axisBindings[(int)currentAxisSet[i]] != "")
            {
                axisValues[(int)currentAxisSet[i]] = Input.GetAxis(axisBindings[(int)currentAxisSet[i]]);
            }
        }
        actionManager.ManageActions(activeActions, axisValues);
    }

    public void SetupBasicKeyBinding()
    {
        keyBindings[(int)ActionManager.Action.moveForward] = new List<KeyCode>() { KeyCode.W };
        keyBindings[(int)ActionManager.Action.moveBackward] = new List<KeyCode>() { KeyCode.S };
        keyBindings[(int)ActionManager.Action.moveLeft] = new List<KeyCode>() { KeyCode.A };
        keyBindings[(int)ActionManager.Action.moveRight] = new List<KeyCode>() { KeyCode.D };
        keyBindings[(int)ActionManager.Action.jump] = new List<KeyCode>() { KeyCode.Space };
        axisBindings[(int)ActionManager.Axis.cameraX] = "Mouse X";
        axisBindings[(int)ActionManager.Axis.cameraY] = "Mouse Y";
    }
}
