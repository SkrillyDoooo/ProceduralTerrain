﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public float ScreenEdgeBorderThickness = 5.0f; // distance from screen edge. Used for mouse movement

    [Header("Camera Mode")]
    [Space]
    public bool RTSMode = true;
    public bool FlyCameraMode = false;

    [Header("Movement Speeds")]
    [Space]
    public float minPanSpeed;
    public float maxPanSpeed;
    public float secToMaxSpeed; //seconds taken to reach max speed;
    public float zoomSpeed;

    [Header("Movement Limits")]
    [Space]
    public bool enableMovementLimits;
    public Vector2 heightLimit;
    public Vector2 lenghtLimit;
    public Vector2 widthLimit;

    private float panSpeed;
    private Vector3 initialPos;
    private Vector3 panMovement;
    private Vector3 pos;
    private Quaternion rot;
    private Vector3 lastMousePosition;
    private Quaternion initialRot;
    private float panIncrease = 0.0f;

    [Header("Rotation")]
    [Space]
    public bool rotationEnabled;
    public float rotateSpeed;

    // Use this for initialization
    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }


    void Update()
    {
        if (!Application.isFocused)
            return;
        # region Camera Mode

        //check that ony one mode is choosen
        if (RTSMode == true) FlyCameraMode = false;
        if (FlyCameraMode == true) RTSMode = false;

        #endregion

        #region Movement

        panMovement = Vector3.zero;

        Vector3 forward = Vector3.Cross(transform.right, Vector3.up).normalized;
        Vector3 right = Vector3.Cross(-transform.forward, Vector3.up).normalized;

        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - ScreenEdgeBorderThickness)
        {
            panMovement += forward * panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= ScreenEdgeBorderThickness)
        {
            panMovement -= forward * panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= ScreenEdgeBorderThickness)
        {
            panMovement -= right * panSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - ScreenEdgeBorderThickness)
        {
            panMovement += right * panSpeed * Time.deltaTime;
            //pos.x += panSpeed * Time.deltaTime;
        }


        panMovement += Input.mouseScrollDelta.y * transform.forward * zoomSpeed * Time.deltaTime;

        if (RTSMode) transform.Translate(panMovement, Space.World);
        else if (FlyCameraMode) transform.Translate(panMovement, Space.Self);

        //increase pan speed
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
            || Input.mouseScrollDelta.y != 0.0f
            || Input.mousePosition.y >= Screen.height - ScreenEdgeBorderThickness
            || Input.mousePosition.y <= ScreenEdgeBorderThickness
            || Input.mousePosition.x <= ScreenEdgeBorderThickness
            || Input.mousePosition.x >= Screen.width - ScreenEdgeBorderThickness)
        {
            panIncrease += Time.deltaTime / secToMaxSpeed;
            panSpeed = Mathf.Lerp(minPanSpeed, maxPanSpeed, panIncrease);
        }
        else
        {
            panIncrease = 0;
            panSpeed = minPanSpeed;
        }

        #endregion

        #region mouse rotation

        if (rotationEnabled)
        {
            // Mouse Rotation
            if (Input.GetMouseButton(1))
            {
                Vector3 mouseDelta;
                if (lastMousePosition.x >= 0 &&
                    lastMousePosition.y >= 0 &&
                    lastMousePosition.x <= Screen.width &&
                    lastMousePosition.y <= Screen.height)
                    mouseDelta = Input.mousePosition - lastMousePosition;
                else
                {
                    mouseDelta = Vector3.zero;
                }
                var rotation = transform.up * Time.deltaTime * rotateSpeed * mouseDelta.x;
                rotation += -transform.right * Time.deltaTime * rotateSpeed * mouseDelta.y;

                transform.Rotate(rotation, Space.World);

                // Make sure z rotation stays locked
                rotation = transform.rotation.eulerAngles;
                rotation.z = 0;
                transform.rotation = Quaternion.Euler(rotation);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (RTSMode) transform.rotation = Quaternion.Slerp(transform.rotation, initialRot, 0.5f * Time.time);
            }

            lastMousePosition = Input.mousePosition;

        }


        #endregion


        #region boundaries

        if (enableMovementLimits == true)
        {
            //movement limits
            pos = transform.position;
            pos.y = Mathf.Clamp(pos.y, heightLimit.x, heightLimit.y);
            //pos.z = Mathf.Clamp(pos.z, lenghtLimit.x, lenghtLimit.y);
            //pos.x = Mathf.Clamp(pos.x, widthLimit.x, widthLimit.y);
            transform.position = pos;
        }



        #endregion

    }
}
