using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCameraManager : MonoBehaviour
{
    public bool active;
    private float horizontalSensi = 100f, verticalSensi = 100f;
    private float phi = 0, theta = 0;
    private float oldPhi = 0, oldTheta = 0;
    private int camControl = 1;
    
    

    // Start is called before the first frame update
    void OnEnable()
    {
        EnableDisable(true);
    }

    private void Update()
    {
        
        //Debug.DrawRay(transform.position, GetLateralVector());
    }

    public (float, float) MoveCamera(in Vector2 moveVector)
    {
        if(active)
        {
            oldTheta = theta;
            oldPhi = phi;
            theta = Mathf.Clamp(theta - (moveVector.y * verticalSensi), -90f, 90f);
            phi = (phi + (moveVector.x * horizontalSensi)) % 360f;
            if (camControl == 0) { //Cam unattached to the character
                transform.localRotation = Quaternion.Euler(theta, phi, 0f);
                return (0, 0);
            }
            else if(camControl == 1)  { //Cam directs character horizontal facing (ground characters)
                transform.localRotation = Quaternion.Euler(theta, 0f, 0f);
                return (0, phi);
            } else { //Cam directs character horizontal and vertical facing (planes, submarines, ...)
                return (theta, phi);
            }
        }
        else {
            return (0, 0);
        }
    }

    public void RevertCamera() {
        if (camControl == 0) { //Cam unattached to the character
            return;
        } else if (camControl == 1) { //Cam directs character horizontal facing (ground characters)
            phi = oldPhi;
        } else { //Cam directs character horizontal and vertical facing (planes, submarines, ...)
            theta = oldTheta;
            phi = oldPhi;
        }
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
        lookVector = transform.rotation * Vector3.forward;
    }

    public void GetLateralVector(out Vector3 lateralVector)
    {
        lateralVector = transform.rotation * Vector3.right;
    }

}
