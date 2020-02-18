using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Stats Variable")]
    protected float health;
    protected float groundSpeed;
    protected int status;
    abstract public void Act(bool[] actions);

}
