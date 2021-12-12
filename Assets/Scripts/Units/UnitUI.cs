using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitUI : MonoBehaviour
{

    [SerializeField]
    private ContextPanelData contextPanelData;

    public ContextPanelData GetContextPanelData()
    {
        return contextPanelData;
    }

    internal void SetInfoPanelRoot(VisualElement root)
    {
    }
}
