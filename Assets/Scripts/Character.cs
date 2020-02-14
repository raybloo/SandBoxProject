using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Stats Variable")]
    protected float health;
    private float groundSpeed;
    private int status;
    abstract public void Act(bool[] actions);

}
