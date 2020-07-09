using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Warpath UI Data/Button Data")]
public class WindowButtonsData : ScriptableObject
{
    public ButtonData[] buttons;
}

[System.Serializable]
public struct ButtonData
{
    public string name;
    public string tooltip;
    public Texture2D icon;
    StyleBackground styleInstance;
    public StyleBackground style
    {
        get {
            if (styleInstance == null)
                styleInstance = new StyleBackground(icon);
            return styleInstance;
        }
    }
}

