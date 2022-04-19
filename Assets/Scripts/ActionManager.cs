using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionManager : MonoBehaviour
{
    
    

    public static readonly int actionsLength = 17;
    public enum Action
    {
        menuDown,
        menuUp,
        menuSelect,
        moveForward,
        moveBackward,
        strafeLeft,
        strafeRight,
        jump,
        attack,
        slide,
        brake,
        boost,
        ability1,
        ability2,
        ability3,
        ultimate,
        melee,
    }

    public static readonly int axisLength = 2;
    public enum Axis
    {
        cameraX,
        cameraY
    }

    [Header("Actions Receiver")]
    public PlayerSuperviser playerSuperviser;
    public Character character;

    [Header("Input Source")]
    public InputManager inputManager;

    private Action[] basicActionSet = new Action[] { Action.moveForward, Action.moveBackward, Action.strafeLeft, Action.strafeRight, Action.jump, Action.attack, Action.slide, Action.ability1 };
    private Axis[] basicAxisSet = new Axis[] { Axis.cameraX, Axis.cameraY };
    
    private Action[] characterActionConverter;
    private Action[] playerActionConverter;
    // Start is called before the first frame update
    void Start()
    {
        fillInConverters();
        inputManager.SetNewActionSet(new Action[0],basicActionSet);
        inputManager.SetNewAxisSet(basicAxisSet);
    }

    public void ManageActions(bool[] activeActions, float[] axisValues)
    {
        bool[] playerActiveActions = new bool[playerActionConverter.Length];
        for(int playerAction = 0; playerAction < playerActionConverter.Length; playerAction++)
        {
            playerActiveActions[playerAction] = activeActions[(int)playerActionConverter[playerAction]];
        }
        playerSuperviser.ManageActions(playerActiveActions);
        bool[] characterActiveActions = new bool[characterActionConverter.Length];
        for (int charAction = 0; charAction < characterActionConverter.Length; charAction++)
        {
            characterActiveActions[charAction] = activeActions[(int)characterActionConverter[charAction]];
        }
        character.Act(characterActiveActions, axisValues);
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
