using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDEQMove : MonoBehaviour
{
    const float speed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float movement = speed * Time.deltaTime;
        Vector3 direction = new Vector3();
        if (Input.GetKey("W")) direction += new Vector3(1, 0, 0);
        if (Input.GetKey("A")) direction += new Vector3(-1, 0, 0);
        if (Input.GetKey("S")) direction += new Vector3(-1, 0, 0);
        if (Input.GetKey("D")) direction += new Vector3(1, 0, 0);
        if (Input.GetKey("E")) direction += new Vector3(1, 0, 0);
        if (Input.GetKey("Q")) direction += new Vector3(-1, 0, 0);
        Vector3 displacement = transform.rotation*(direction.normalized * movement);
        transform.position += displacement;
    }
}
