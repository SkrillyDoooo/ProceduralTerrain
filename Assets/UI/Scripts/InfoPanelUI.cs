using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UIElements;

public class InfoPanelUI 
{

    VisualElement headerImage;
    Label headerName;
    VisualElement details;
    public InfoPanelUI(VisualElement root)
    {
        headerImage = root.Query<VisualElement>(name: "header-image");
        headerName = root.Query<Label>(name: "header-name");
        details = root.Query<VisualElement>(name: "details");
    }

    public VisualElement SetInfoPanel(Texture2D texture2D, string label, VisualTreeAsset asset)
    {
        details.Clear();
        headerImage.style.backgroundImage = texture2D;
        headerName.text = label;

        asset.CloneTree(details);
        headerImage.visible = true;
        headerName.visible = true;
        details.visible = true;

        return details;
    }

    public void DisableInfoPanel()
    {
        headerImage.visible = false;
        headerName.visible = false;
        details.Clear();
    }
}
