using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInput : MonoBehaviour
{
    private static Plane terrainPlane = new Plane(Vector3.up, Vector3.zero);

    public static bool TryGetCursorRaycastPoint(out Vector3 point)
    {
        point = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        if (terrainPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            point = hitPoint;
            return true;
        }
        else
        {
            return false;
        }
    }
}
