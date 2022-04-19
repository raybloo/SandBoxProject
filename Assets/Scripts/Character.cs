using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : Movable
{
    public static readonly int actionsLength = 14;
    public enum Action
    {
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

    public enum MoveState
    {
        ground,
        fly,
        jump,
        knock,
        drive,
        cinematic
    }

    public enum State
    {
        alive,
        stunned,
        silenced,
        dead
    }

    [Header("Stats Variable")]
    public float health;
    public float speedCap;
    public State status;
    public Character killer;
    public Animator characterAnimator;

    abstract public void Act(bool[] actions, float[] axis);

    public void Damage(Character source, float amount) {
        health -= amount;
        if(health < 0f) {
            health = 0f;
            status = State.dead;
        }
    }
}
