using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCursor : MonoBehaviour
{

    void Start()
    {

    }

    // Update is called once per frame

    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
    }

    public void Show()
    {
        GetComponent<Renderer>().enabled = true;
    }
}
