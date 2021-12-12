using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarUI : MonoBehaviour
{
    private VisualElement healthBarElement;
    int poolIndex = -1;
    private VisualElement healthBarValue;
    StyleLength width;
    public int pixelHeightOffset = 64;

    bool isEnabled
    {
        get
        {
            return (poolIndex != -1);
        }
    }

    private void Start()
    {
        width = new StyleLength(new Length(100, LengthUnit.Percent));
    }

    public void Enable()
    {
        if(!isEnabled)
        {
            healthBarElement = Game.Instance.RetrieveHealthBarFromPool(out poolIndex);
            if(isEnabled) // was the health bar actually retrieved?
            {
                healthBarValue = healthBarElement.Query<VisualElement>(className: "health-value");
                healthBarValue.style.width = width;
            }
        }
    }

    private void Update()
    {
        if(isEnabled)
        {
            Vector3 v3 = Camera.main.WorldToScreenPoint(transform.position);
            v3.z = 0;
            v3.x -= 32;
            v3.y = Screen.height - v3.y - pixelHeightOffset;
            healthBarElement.transform.position = v3;
        }
    }

    public void UpdateWidthPercent(float perc)
    {
        if(isEnabled)
            width = perc;
    }

    public void Disable()
    {
        if(isEnabled)
        {
            Game.Instance.ReturnHealthBarToPool(poolIndex);
            poolIndex = -1;
        }
    }
}
