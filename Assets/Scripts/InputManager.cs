using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public PlayerManager playerManager;

    public KeyCode MoveForward = KeyCode.W;
    public KeyCode MoveBackward = KeyCode.S;
    public KeyCode StrafLeft = KeyCode.A;
    public KeyCode StrafRight = KeyCode.D;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(MoveForward))
        {

        }

        if (Input.GetKey(MoveBackward))
        {

        }

        if (Input.GetKey(StrafLeft))
        {

        }

        if (Input.GetKey(StrafRight))
        {

        }
    }
}
