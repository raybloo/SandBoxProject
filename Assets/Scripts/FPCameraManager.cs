using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCameraManager : MonoBehaviour
{
    public bool active;
    private float phi = 0, theta = 0;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        EnableDisable(true);
    }

    private void Update()
    {
        MoveCamera(new Vector2(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y")));
    }

    public void MoveCamera(Vector2 moveVector)
    {
        if(active)
        {
            phi = (phi + moveVector.x) % 360f;
            theta = Mathf.Max(Mathf.Min(90f, theta - moveVector.y), -90f);
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

}
