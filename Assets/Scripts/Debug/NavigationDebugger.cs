using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationDebugger : MonoBehaviour
{

    public enum NavigationDebuggerState
    {
        SelectStartingLocation,
        NavigatingToCursor
    }

    private NavigationDebuggerState state = NavigationDebuggerState.SelectStartingLocation;

    public bool active;

    public List<NavigationNode> node;

    public Transform startingLocationObjectTransform;

    // Update is called once per frame
    void Update()
    {

        if(active)
        {
            switch(state)
            {
                case NavigationDebuggerState.SelectStartingLocation:
                    UpdateSelectStartingLocation();
                    break;
                case NavigationDebuggerState.NavigatingToCursor:
                    break;
                default:
                    break;
            }
        }
    }

    void UpdateSelectStartingLocation()
    {
        if(TerrainInput.TryGetCursorRaycastPoint(out var raycastPosition))
        {
            startingLocationObjectTransform.position = raycastPosition;
        }
    }
}
