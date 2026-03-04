using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public ERoomType[,] levelmap;
    public Stack<Vector2Int> endRooms;

    public LevelData(int width, int height)
    {
        levelmap = new ERoomType[width, height];
        endRooms = new Stack<Vector2Int>();
        
        ResetMap();
    }

    public LevelData(ERoomType[,] map)
    {
        levelmap = map;
    }

    public void ResetMap()
    {
        endRooms.Clear();

        for (int i = 0; i < levelmap.GetLength(1); i++)
        {
            for (int j = 0; j < levelmap.GetLength(0); j++)
            {
                levelmap[j, i] = ERoomType.Free;
            }
        }
    }
}

