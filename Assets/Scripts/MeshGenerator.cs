using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class MeshGenerator
{
	///TODO 2 - GenerateTerrainMesh
	#region MeshGeneration
	//We add the heightMultiplier to make sure that the 3dMesh we are making, actually has some height to it, and not just 0-1, but mountains etc
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
	{
		//Assign height
		AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
		//The width is equal the height of it in the 0 and 1st dimension, x & y
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;
		//If the levelOfDetail is 0, set it to 1, else times 2
		int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;
		//Set the amount of verticesn in our mesh
		MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
		int vertexIndex = 0;
		for (int y = 0; y < height; y += meshSimplificationIncrement)
		{
			for (int x = 0; x < width; x += meshSimplificationIncrement)
			{
				//Evaluate return the value of the heightcurve at the given point in time when the method/loop is run
				//We then multiply it with the heightmultiplier to get our lovely mesh
				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}
				//We increment the index of our vertices by one at the end of each loop, we keep track of where we are in our 1d array
				vertexIndex++;
			}
		}
		//We return meshdata instead of mesh, as we are implementing threading so the game does not freeze when we are generating chunks on the fly 
		return meshData;
	}
	#endregion
}
public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}
	//Convinience Method if we want to add a triangle
	public void AddTriangle(int a, int b, int c)
	{
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}

}