using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldSpaceUI : MonoBehaviour
{
    private static WorldSpaceUI m_Instance;
    public static WorldSpaceUI Instance
    {
        get
        {
            if(m_Instance == null)
            {
                m_Instance = FindObjectOfType<WorldSpaceUI>();
            }
            return m_Instance;
        }
    }



    private VisualElement root;
    
    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
    }

    public VisualElement AddElement(VisualTreeAsset vta)
    {
        vta.CloneTree(root);
        return root;
    }

    internal void RemoveElement(VisualElement healthBarElement)
    {
        root.Remove(healthBarElement);
    }
}
