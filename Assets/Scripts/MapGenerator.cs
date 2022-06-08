using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;


public class MapGenerator : MonoBehaviour
{
	//Drawmode for the different maps we have. 
	public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap };
	public DrawMode drawMode;
	public Noise.NormalizeMode normalizeMode;

	public const int mapChunkSize = 241;
	//The reason we define the range of 0-6 instead of 0-12, is that we only use even numbers, and only use the level of detail, when our i-value is not 1. 
	[Range(0, 6)]
	public int editorPreviewLOD;
	public float noiseScale;

	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	///The seed is essentialy a key to a specific generated world. If one were to have
	///the same start position, and the same values when is came to octaves, persistance, lacunarity etc, as well as the 'key' that
	///is the seed, they would get a completely identically generated world. 
	public int seed;
	public Vector2 offset;
	//whether or not we'd like to use the falloffmap
	public bool useFalloff;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	//Unless we are testing anything, this should always be on.
	public bool autoUpdate;
	//The defined regions, sand, water etc.
	public TerrainType[] regions;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
	#region Drawmodes
	//Different drawmodes. 
	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData(Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		}
		else if (drawMode == DrawMode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		}
		else if (drawMode == DrawMode.FalloffMap)
        {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GenerateFallOffMap(mapChunkSize)));
        }
	}
	#endregion
	#region Threading and Requesting
	public void RequestMapData(Vector2 centre, Action<MapData> callback)
	{
		ThreadStart threadStart = delegate {
			MapDataThread(centre, callback);
		};

		new Thread(threadStart).Start();
	}

	void MapDataThread(Vector2 centre, Action<MapData> callback)
	{
		MapData mapData = GenerateMapData(centre);
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}


	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread(mapData, lod, callback);
		};

		new Thread(threadStart).Start();
	}


	void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}
	#endregion
	///TODO 2 - GenerateMapData
	#region MapDataGeneration
	MapData GenerateMapData(Vector2 centre)
	{
		//We call the GenerateNoiseMap, and afterwards assign 'heightvalues' or colours to represent the height, to the noisemap
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++)
		{
			for (int x = 0; x < mapChunkSize; x++)
			{
				///Because of the changes we have amde to our noise-function
				///we can no longer assume that current-height will be in the range 0-1
				///It would be nice however, if we can at least assume that it is not 
				///less than 0. 
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++)
				{
					//Here we assign the colours to the different heights(values) of the map. 
					if (currentHeight >= regions[i].height)
					{
						colourMap[y * mapChunkSize + x] = regions[i].colour;
					}
					///We only break once we've reached a currenheight-value
					///of less than the regions-height
					else
					{
						break;
                    }
				}
			}
		}
		//Return MapData
		return new MapData(noiseMap, colourMap);
	}
	#endregion
	//Called automatically whenever a value in this script is changed - Error handling
	void OnValidate()
	{
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0)
		{
			octaves = 0;
		}
	}
	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}
[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}
//MapData is built of a heightmap as well as a colourmap, as these two are linked, one does not work without the other without a significant amount of refacotoring
public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

	public MapData(float[,] heightMap, Color[] colourMap)
	{
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}
