using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using System.IO;

public class TextureGeneratorWindow : EditorWindow
{
    string path;

    public float intensity = 2.0f;
    public float period = 0.5f;

    [MenuItem("Window/Texture Generator")]
    public static void ShowWindow()
    {
        TextureGeneratorWindow gw = EditorWindow.GetWindow(typeof(TextureGeneratorWindow)) as TextureGeneratorWindow;
        gw.path = Application.dataPath + "/Textures";
    }

    private void OnGUI()
    {
        intensity = EditorGUILayout.FloatField("Intensity: ", intensity);
        period = EditorGUILayout.FloatField("Period: ", period);


        if (GUILayout.Button("Generate Textures"))
        {
            if(!Directory.Exists(path))
            {
                Debug.LogError("Texture path does not exist.");
                return;
            }

            var texture = new Texture2D(256, 256);
            for(int i = 0; i < texture.width; i++)
            {
                for(int j = 0; j <  texture.height; j++)
                {
                    var n = math.unlerp(-1.0f, 1.0f, noise.cnoise(new float2(i,j) / (texture.width/2.0f)));
                    texture.SetPixel(i, j, new Color(n, n, n));
                }
            }

            texture.Apply();

            File.WriteAllBytes(path + "/sample.png", texture.EncodeToJPG());

            AssetDatabase.Refresh();
        }
    }
}
