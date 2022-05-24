using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//We are making it static and removing the Mono-reference as we are not attaching it to a gameobjcet, nor are we making multiple instances of it
public static class Noise
{
	public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;
		//At the end of this loop, we will have found the maximum possible height-value.
		for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}
        
        //Here we handle the scale-value being 0. If it is, we get a division by 0 error when making our sample-x/y values - Error handling
        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        //This lets us zoom into the middle of the noisemap when changing the scale, instead of the top right corner - Convinience
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				//We reset these values to one, for a fresh start so to speak
				amplitude = 1;
				frequency = 1; 
				float noiseHeight = 0;

				for (int i = 0; i < octaves; i++)
				{
					///To prevent the landmasses changing shape as we increase/decrease the offset, we want it to be affected by the scale and frequency.
					///We therefore put them inside the parentheses 
					///Usability. We'd like for the map to be 'static' once generated.
					///As in, you should be able to go back to the formerly generated masses, by for instance moving backwards from your current point.
					float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency ;
					float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

					///We are creating noiseHeight, by multiplying perlinvalue with amplitude, for each octave. 
					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalNoiseHeight)
				{
					maxLocalNoiseHeight = noiseHeight;
				}
				else if (noiseHeight < minLocalNoiseHeight)
				{
					minLocalNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}
		///Here we normalize the map-values between 0 & 1
		///The reason that the seams of our chunks do not line up perfectly,
		///is because the InverseLerp-method, is run for each chunk,
		///giving them slightly different values for min-and maxNoiseHeight
		///The method underneath would still be the preferred way of doing the value-normalisation, if we were not going for an endless-terrain system. 
		///Because if we generate the entire map at once, then we know exactly what the min and max-values are and we can make certain that they are used. 
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				if(normalizeMode == NormalizeMode.Local)
                {
					noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
				}
                else 
                {
					float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
					noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
				
			}
		}

		return noiseMap;
	}
}
