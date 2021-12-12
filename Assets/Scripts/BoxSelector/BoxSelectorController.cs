using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoxSelectorController 
{

    public event Action<Bounds> OnReceiveBounds;
    BoxSelectorModel model;
    Camera cam;
    public BoxSelectorController(BoxSelectorUI ui)
    {
        model = new BoxSelectorModel(ui);
        model.SetActive(false);
        ui.RegisterInputEvents(MouseDown, MouseUp, MouseMove);

        cam = Camera.main;
    }

    public void MouseDown(MouseDownEvent evt)
    {
        if(evt.button == 0)
        {
            model.SetActive(true);
            model.SetStartMousePos(evt.mousePosition);
        }
    }

    public void MouseUp(MouseUpEvent evt)
    {
        if(evt.button == 0)
        {
            model.SetActive(false);
            OnReceiveBounds(model.GetSelectionBoxViewportBounds(cam));
        }

    }

    public void MouseMove(MouseMoveEvent evt)
    {
        if(evt.button == 0)
        {
            model.UpdateCurrentMosPos(evt.mousePosition);
        }
    }

}
