using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoxSelectorUI 
{
    VisualElement root;
    VisualElement middle;
    VisualElement selectorBox;
    public int width { get; private set; }
    public int height { get; private set; }


    public BoxSelectorUI(VisualElement root, VisualElement middle)
    {
        this.root = root;
        this.middle = middle;

        selectorBox = root.Query<VisualElement>(name: "selector");
    }

    public void RegisterInputEvents(EventCallback<MouseDownEvent> mouseDown, EventCallback<MouseUpEvent> mouseUp,  EventCallback<MouseMoveEvent> mouseMove)
    {
        root.RegisterCallback<MouseUpEvent>(mouseUp);
        middle.RegisterCallback<MouseDownEvent>(mouseDown);
        root.RegisterCallback<MouseMoveEvent>(mouseMove);
    }

    internal void SetActive(bool enabled)
    {
        selectorBox.visible = enabled;
    }

    internal void UpdateSelectorBox(Rect rect)
    {
        Rect middleBound = middle.worldBound;

        if(rect.y < middleBound.y)
        {
            rect.height -= ((int)middleBound.y - (int)rect.y);
            rect.y = middleBound.y;
        }

        if (rect.yMax > middleBound.yMax)
        {
            rect.height = (int)middleBound.yMax - (int)rect.y;
        }

        var localRect = middle.WorldToLocal(rect);
        selectorBox.style.left = localRect.x;
        selectorBox.style.top = localRect.y;

        selectorBox.style.width = localRect.width;
        selectorBox.style.height = localRect.height;
    }
}
