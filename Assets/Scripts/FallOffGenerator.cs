using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOffGenerator
{
    public static float[,] GenerateFallOffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                ///The reason we multiply by 2 and minus 1 is to
                ///get a range of -1 to 1
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                ///Here we try and figure out which, x or y, is closest to the edge of our chunk
                ///The closest one will be used for our falloff-map
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = value;
            }
        }
        return map;
    }

}
