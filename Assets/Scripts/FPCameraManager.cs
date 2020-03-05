using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCameraManager : MonoBehaviour
{
    public bool active;
    private float horizontalSensi = 100f, verticalSensi = 100f;
    private float phi = 0, theta = 0;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        EnableDisable(true);
    }

    private void Update()
    {
        
        //Debug.DrawRay(transform.position, GetLateralVector());
    }

    public void MoveCamera(Vector2 moveVector)
    {
        if(active)
        {
            phi = (phi + (moveVector.x * horizontalSensi)) % 360f;
            theta = Mathf.Max(Mathf.Min(89.5f, theta - (moveVector.y * verticalSensi)), -89.5f);
            transform.localRotation = Quaternion.Euler(theta, phi, 0f);
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

    public Vector3 GetLookVector()
    {
        return transform.rotation * Vector3.forward;
    }

    public Vector3 GetLateralVector()
    {
        return transform.rotation * Vector3.right;
    }

}
