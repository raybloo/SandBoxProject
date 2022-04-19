using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLineGravity : GravityManager
{
    public Vector3 circleCenter;
    public float circleRadius;
    public Vector3 projectionVector = new Vector3(1, 1, 1); //x axis
    public Collider collider;

    private Vector3 radiusVector;
    private Vector3 target;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool CheckValidity(in Vector3 position) {
        if (collider) {
            return collider.bounds.Contains(position);
        } else {
            return true;
        }
    }

    override public void GetGravityDir(in Vector3 position, ref Vector3 gravityDir) {
        radiusVector = circleCenter - position;
        radiusVector.Scale(projectionVector);
        if (radiusVector == Vector3.zero) {
            gravityDir = Vector3.down;
        } else {
            target = circleCenter - (radiusVector.normalized * circleRadius);
            gravityDir = position - target;
            if (gravityAttracts) {
                gravityDir = -gravityDir;
            }
            gravityDir.Normalize();
        }
    }
}
