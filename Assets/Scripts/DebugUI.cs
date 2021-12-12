using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugUI : MonoBehaviour
{
    Image item;
    VisualElement debugRT;
    Label label;
    public void SetRenderTexture(RenderTexture rt)
    {
        InitImage();
        item.image = rt;
        item.image.width = rt.width;
        item.image.height = rt.height;

        item.style.width = 176;
        item.style.height = 176;
    }

    public void SetCoordinateLabel(int x, int y)
    {
        label.text = $"{x}:{y}";
    }

    void InitImage()
    {
        if (item != null)
            return;

        var uiDocument = GetComponent<UIDocument>();
        item = new Image();
        label = new Label();
        var root = uiDocument.rootVisualElement;
        root = root.Query(name: "root");
        root.Add(item);
        root.Add(label);
    }
    // Start is called before the first frame update
    void Start()
    {
        InitImage();
    }
}
