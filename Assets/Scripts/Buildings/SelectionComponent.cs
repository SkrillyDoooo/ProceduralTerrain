using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionComponent : MonoBehaviour
{
    Projector projector;
    void Start()
    {
        projector = GetComponent<Projector>();
    }

    public void SetProjectorActive(bool active)
    {
        if(projector == null)
            projector = GetComponent<Projector>();
        projector.enabled = active;
    }
}
