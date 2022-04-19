using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravityManager : MonoBehaviour
{
    public bool gravityAttracts;

    public abstract void GetGravityDir(in Vector3 position, ref Vector3 gravityDir);

    public abstract bool CheckValidity(in Vector3 position);
}
