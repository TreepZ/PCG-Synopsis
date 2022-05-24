using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    public void DrawTexture(Texture2D texture)
    {
        //Here we set the textureRenderes shared-mat to the texture we've just rendered so we can preview our maps without entering gamemode
        textureRenderer.sharedMaterial.mainTexture = texture;
        //Here we set the size of the plane as the same size of the map
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
   public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
