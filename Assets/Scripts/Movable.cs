using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movable : MonoBehaviour
{
    [Header("Manager Variable")]
    public GravityManager currentGravityManager;
    public GravityManager[] gravityManagers;
    public int gravityManagerCount = 1;

    protected void SelectGravityManager(in Vector3 position) {
        for(int i = 0; i < gravityManagerCount; i++) {
            if(gravityManagers[i].CheckValidity(position)) {
                currentGravityManager = gravityManagers[i];
                return;
            }
        }
        currentGravityManager = null;
    }
}
