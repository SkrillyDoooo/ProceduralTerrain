using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdateableData
{
    public NoiseSettings noiseSettings;

    public bool applyFalloffMap;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITR
    protected override void OnValidate()
    {
        base.OnValidate();
        noiseSettings.ValidateValues();
    }
#endif
}
