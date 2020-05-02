using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationCurveExtension
{

    public static int CurveArrayLength = 256;
    public static float[] GenerateCurveArray(this AnimationCurve self)
    {
        float[] returnArray = new float[CurveArrayLength];
        for (int j = 0; j < CurveArrayLength; j++)
        {
            returnArray[j] = self.Evaluate(j / (float)CurveArrayLength);
        }
        return returnArray;
    }
}
