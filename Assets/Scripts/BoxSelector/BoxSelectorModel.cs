using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class BoxSelectorModel
{
    private BoxSelectorUI m_View;
    public Vector2 mosPosStart { get; set; }
    public Vector2 mosPosCurrent { get; set; }
    public bool enabled { get; private set; }

    private Bounds bounds;

    private Rect rect;


    public BoxSelectorModel(BoxSelectorUI view)
    {
        m_View = view;
    }

    internal void SetStartMousePos(Vector3 mousePosition)
    {
        mosPosStart = mousePosition;
        m_View.UpdateSelectorBox(new Rect(mousePosition.x, mousePosition.y, 0,0));
    }

    public void SetActive(bool enabled)
    {
        this.enabled = enabled;
        m_View.SetActive(enabled);
    }

    public Bounds GetSelectionBoxViewportBounds(Camera camera)
    {

        var start = new Vector2(mosPosStart.x, Screen.height - mosPosStart.y);
        var end = new Vector2(mosPosCurrent.x, Screen.height - mosPosCurrent.y);

        var v1 = camera.ScreenToViewportPoint(start);
        var v2 = camera.ScreenToViewportPoint(end);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public void UpdateCurrentMosPos(Vector2 currentMousePos)
    {
        mosPosCurrent = currentMousePos;

        var topLeft = Vector3.Min(mosPosStart, currentMousePos);
        var bottomRight = Vector3.Max(mosPosStart, currentMousePos);
        // Create Rect
        rect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        m_View.UpdateSelectorBox(rect);
    }
}
