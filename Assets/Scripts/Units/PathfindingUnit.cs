using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingUnit : MonoBehaviour
{

    private Vector3 TargetPosition { get; set; }
    public float speed = 1.0f;
    private bool moving = true;
    // Update is called once per frame

    public void SetTargetPosition(Vector3 pose)
    {
        TargetPosition = pose;
        moving = true;
    }
    void Update()
    {
        if (!moving)
            return;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, Time.deltaTime * speed);
        if (transform.position == TargetPosition) moving = false;
    }
}
