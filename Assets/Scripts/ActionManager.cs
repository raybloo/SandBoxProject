using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionManager : MonoBehaviour
{
    public static readonly int actionsLength = 30;
    public static readonly int axisLength = 2;

    public enum Action
    {
        menuDown,
        menuUp,
        menuSelect,
        moveForward,
        moveBackward,
        moveLeft,
        moveRight,
        jump,
    }

    public enum Axis
    {
        cameraX,
        cameraY
    }

    [Header("Actions Receiver")]
    public PlayerSuperviser playerSuperviser;
    public Character character;

    [Header("Actions Receiver")]
    public InputManager keyboardManager;

    private Action[] exploreActionSet = new Action[] { Action.moveForward, Action.moveBackward, Action.moveLeft, Action.moveRight};
    
    private Action[] characterActionConverter;
    private Action[] playerActionConverter;
    // Start is called before the first frame update
    void Start()
    {
        keyboardManager.SetNewActionSet(new Action[0],exploreActionSet);
    }

    void fillInConverters()
    {
        characterActionConverter = new Action[Enum.GetNames(typeof(Character.Action)).Length];
        foreach(Character.Action charAction in Enum.GetValues(typeof(Character.Action)))
        {
            foreach (Action regAction in Enum.GetValues(typeof(Action)))
            {
                if (Enum.GetName(typeof(Character.Action),charAction) == Enum.GetName(typeof(Action), regAction))
                {
                    characterActionConverter[(int)charAction] = regAction;
                }
            }
        }
        playerActionConverter = new Action[Enum.GetNames(typeof(PlayerSuperviser.Action)).Length];
        foreach (PlayerSuperviser.Action playerAction in Enum.GetValues(typeof(PlayerSuperviser.Action)))
        {
            foreach (Action regAction in Enum.GetValues(typeof(Action)))
            {
                if (Enum.GetName(typeof(PlayerSuperviser.Action), playerAction) == Enum.GetName(typeof(Action), regAction))
                {
                    playerActionConverter[(int)playerAction] = regAction;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
