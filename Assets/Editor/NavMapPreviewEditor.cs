using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavMapPreview))]
public class NavMapPreviewEditor : Editor
{

    bool foldout = false;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        NavMapPreview mapPreview = (NavMapPreview)target;
        SerializedProperty property = serializedObject.FindProperty("renderLevels");
        DrawArrayWithoutSize(mapPreview, property, mapPreview.m_Levels);
        serializedObject.ApplyModifiedProperties();
    }

    public void DrawArrayWithoutSize(NavMapPreview mapPreview, SerializedProperty Property, int ArraySize)
	{
		if (foldout = EditorGUILayout.Foldout(foldout, "Render Levels"))
		{
			EditorGUI.indentLevel++;

			for (int I = 0; I < ArraySize; ++I)
			{

				SerializedProperty ElementProperty = Property.GetArrayElementAtIndex(I);
				EditorGUILayout.PropertyField(ElementProperty, new GUIContent(string.Concat("Render Level: ", I + 1)));
			}

			EditorGUI.indentLevel--;
		}
	}
}
