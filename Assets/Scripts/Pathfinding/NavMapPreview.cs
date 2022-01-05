using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavMapPreview : MonoBehaviour
{
    NavigationNodePool m_NavNodePool;

    private static Color[] colors = {Color.red, Color.cyan, Color.blue};

    [HideInInspector]
    public int m_Levels;

    public bool[] renderLevels;

    public float m_Scale;
    
    public int m_Dimensions;

    public void SetNavMap(NavigationNodePool navNodePool, float scale, int levels, int dimensions)
    {
        m_NavNodePool = navNodePool;
        m_Levels = levels;
        if(renderLevels.Length != m_Levels)
        {
            bool[] tmp = new bool[m_Levels];
            for(int i = 0; i < renderLevels.Length && i < m_Levels; i++)
            {
                tmp[i] = renderLevels[i];
            }
            renderLevels = tmp;
        }
        m_Scale = scale;
        m_Dimensions = dimensions;
    }

    void OnDrawGizmos()
    {
        if(m_NavNodePool == null)
            return;
        bool shouldRender = false;
        foreach(var renderLevel in renderLevels)
        {
            if(renderLevel)
                shouldRender = true;
        }

        if(!shouldRender)
            return;

        foreach(var kvp in m_NavNodePool.GetNodes())
        {
            NavigationNodeID id = kvp.Key;
            NavigationNode node = kvp.Value;
            int level = node.m_id.level;
            if(renderLevels[level])
            {
                Gizmos.color = colors[level % colors.Length];
                

                Vector3 position = node.GetPositionFromNodeCoordinate(m_Scale, m_Dimensions);
                float size = Remap(level, 0, m_Levels, 0.1f, 1.0f);
                Gizmos.DrawSphere(position, size);
#if UNITY_EDITOR
                //Handles.Label(position + Vector3.up * 2, node.m_id.coordinate.ToString());
#endif
                foreach(var neighbor in node.GetNeighbors())
                {
                    Vector3 neighborPosition =  neighbor.GetPositionFromNodeCoordinate(m_Scale, m_Dimensions);
                     Gizmos.DrawLine(position, neighborPosition);
                }
            }
        }
    } 

    public float Remap (float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
