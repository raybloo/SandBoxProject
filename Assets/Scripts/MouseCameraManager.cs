using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCameraManager : MonoBehaviour
{

    public bool active;
    private float horizontalSensi = 100f, verticalSensi = 100f;
    private float phi = 0, theta = 0;
    private float relativePhi = 0, relativeTheta = 0;
    private float oldPhi = 0, oldTheta = 0;




    // Start is called before the first frame update
    void OnEnable()
    {
        EnableDisable(true);
    }

    private void Update()
    {
        //GetLateralVector(out Vector3 lateralVector);
        //Debug.DrawRay(transform.position, lateralVector);
    }

    //Cam moves without influencing character's orientation (free cam)
    public void MoveFreeCamera(in Vector2 moveVector)
    {
        if(active)
        {
            theta = Mathf.Clamp(theta - (moveVector.y * verticalSensi), -90f, 90f);
            phi = (phi + (moveVector.x * horizontalSensi)) % 360f;
            transform.localRotation = Quaternion.Euler(theta - oldTheta, phi - oldPhi, 0f);
            //relativeTheta = theta - oldTheta;
            //oldTheta = theta;
            //relativePhi = phi - oldPhi;
            //oldPhi = phi;
        }
    }

    //Cam directs character horizontal facing (ground characters)
    public float MoveHorizontalCamera(in Vector2 moveVector) {
        if (active) {
            theta = Mathf.Clamp(theta - (moveVector.y * verticalSensi), -90f, 90f);
            phi = (phi + (moveVector.x * horizontalSensi)) % 360f;
            transform.localRotation = Quaternion.Euler(theta - oldTheta, 0f, 0f);
            //relativeTheta = theta - oldTheta;
            //oldTheta = theta;
            relativePhi = phi - oldPhi;
            oldPhi = phi;
            return relativePhi;
        } else {
            return 0;
        }
    }

    //Cam directs character horizontal and vertical facing (planes, submarines, ...)
    public (float, float) MoveCharacterCamera(in Vector2 moveVector) {
        if (active) {
            theta = Mathf.Clamp(theta - (moveVector.y * verticalSensi), -90f, 90f);
            phi = (phi + (moveVector.x * horizontalSensi)) % 360f;
            relativeTheta = theta - oldTheta;
            oldTheta = theta;
            relativePhi = phi - oldPhi;
            oldPhi = phi;
            return (relativeTheta, relativePhi);
        }   
        return (0, 0);
    }

    public void RevertCamera() {
        //theta = oldTheta;
        phi = oldPhi;
    }

    public void EnableDisable(bool enable)
    {
        if(enable)
        {
            active = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            active = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void GetLookVector(out Vector3 lookVector)
    {
        lookVector = transform.forward;
    }

    public void GetLateralVector(out Vector3 lateralVector)
    {
        lateralVector = transform.right;
    }

    public void GetPosition(out Vector3 position) {
        position = transform.position;
    }

}
