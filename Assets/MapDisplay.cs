using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;


    public bool DisableOnEnterPlaymode;
    private void Start()
    {
        meshRenderer.gameObject.SetActive(!DisableOnEnterPlaymode);
        textureRenderer.gameObject.SetActive(!DisableOnEnterPlaymode);
    }

    public void DrawTexture(Texture2D texture, bool scaleObject)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        if(scaleObject)
            textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData)
    {
        MapGenerator map = FindObjectOfType<MapGenerator>();
        meshFilter.sharedMesh = meshData.CreateMesh();
    }
}
