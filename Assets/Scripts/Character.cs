using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public static readonly int actionsLength = 5;
    public enum Action
    {
        moveForward,
        moveBackward,
        moveLeft,
        moveRight,
        jump,
        crouch,
        attack
    }

    public static readonly int axisLength = 2;
    public enum Axis
    {
        cameraX,
        cameraY
    }

    public enum MoveState
    {
        ground,
        fly,
        jump,
        knock,
        drive,
        cinematic
    }


    [Header("Stats Variable")]
    protected float health;
    protected float groundSpeed;
    protected int status;
    abstract public void Act(bool[] actions, float[] axis);

}
