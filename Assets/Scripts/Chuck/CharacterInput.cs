using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterInput : MonoBehaviour
{
    CharacterController controller;
    public float speed = 1;
    public float pitchSensitivity = 10;
    public float yawSensitivity = 10;

    public float gravity = 9.82f;
    public float jumpHeight = 10;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movementDirection = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
        {
            movementDirection += transform.forward;
        }

        if(Input.GetKey(KeyCode.A))
        {
            movementDirection -= transform.right;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movementDirection += transform.right;
        }

        if (Input.GetKey(KeyCode.S))
        {
            movementDirection -= transform.forward;
        }
        float totalUpV = gravity; 
        if (controller.isGrounded)
        {
            totalUpV = 0;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                totalUpV = -jumpHeight;
            }
        }

        controller.SimpleMove(Vector3.down * totalUpV + movementDirection.normalized * speed);
        float pitch = Input.GetAxis("Mouse Y") * -pitchSensitivity * Time.deltaTime;
        float yaw = Input.GetAxis("Mouse X") * yawSensitivity * Time.deltaTime;

        transform.Rotate(pitch, 0, 0, Space.Self);
        transform.Rotate(0, yaw, 0, Space.World);
    }
}
