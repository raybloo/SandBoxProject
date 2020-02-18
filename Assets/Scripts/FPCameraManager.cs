using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCameraManager : MonoBehaviour
{
    [SerializeField]
    private bool active = false;
    public bool Active
    {
        get
        {
            return active;
        }
        set {
            if(value == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.lockState = CursorLockMode.None;
                mousePos = Input.mousePosition;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
            active = value;
        }
    }

    public Transform cameraTransform;

    
    private Vector3 mousePos;
    // Start is called before the first frame update
    void Start()
    {
        Active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(active)
        {
            Debug.Log(Input.mousePosition - mousePos);
            Cursor.lockState = CursorLockMode.Locked;
            mousePos = Input.mousePosition;
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
