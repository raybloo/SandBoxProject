using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSuperviser : MonoBehaviour
{
    public float speed = 20f;
    public bool moving = false;
    public Vector3 moveVector = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(moving)
        {
            transform.position += moveVector;
        }
    }

}
