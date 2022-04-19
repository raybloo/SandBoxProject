using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentralAxisGravity : GravityManager
{
    public Vector3 projectionVector = new Vector3(0, 1, 1); //x axis
    public Vector3 offset = new Vector3(0, 0, 0);
    public Collider collider;

    private Movable movable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CheckValidity(in Vector3 position) {
        if(collider) {
            return collider.bounds.Contains(position);
        } else {
            return true;
        }
    }

    override public void GetGravityDir(in Vector3 position, ref Vector3 gravityDir) {
        gravityDir = position - offset;
        gravityDir.Scale(projectionVector);
        if (gravityDir.magnitude == 0) {
            gravityDir = Vector3.down;
        } else {
            if(gravityAttracts) {
                gravityDir = -gravityDir;
            }
            gravityDir.Normalize();
        }
    }

}

