using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectAllTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ProductionBuilding[] pd = FindObjectsOfType<ProductionBuilding>();
        foreach(var p in pd)
        {
            PlayerManifest.Instance.AddBuilding(p.transform);
        }
    }
}
