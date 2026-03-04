using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class DungeonGeneratorSingleThread
{
    private int width = 9;
    private int height = 8;
    private int levelCount = 1;
    private int minRoomCount = 7;

    int maxIteration = 500;
    int iterationsCount = 0;

    Stopwatch stopwatch = new Stopwatch();

    Dictionary<int, LevelData> levelmaps;

    public DungeonGeneratorSingleThread(int width, int height, int levelCount, int roomCount, Dictionary<int, LevelData> levelmaps, int seed)
    {
        this.width = width;
        this.height = height;
        this.levelCount = levelCount;
        this.minRoomCount = roomCount;
        this.levelmaps = levelmaps;
        Random.InitState(seed);
    }

    public void StartSingleThread()
    {
        stopwatch.Start();
        for (int i = 0; i < levelCount; i++)
        {
            levelmaps.Add(i, GenerateLevelMap(i + 1));
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
    }

    private LevelData GenerateLevelMap(int level)
    {
        LevelData levelData = new LevelData(width, height);
        int roomCountToGenerate = (int)Mathf.Clamp((Random.Range(0, 2) + minRoomCount - 2 + level * 2.6f), minRoomCount, width * height / 5);
        Vector2Int startCoord = new Vector2Int(width, height) / 2;

        bool isMapValid = false;
        iterationsCount = 0;
        while (!isMapValid && iterationsCount < maxIteration)
        {
            levelData.ResetMap();
            int generatedRooms = GenerateRooms(roomCountToGenerate, startCoord, levelData);
            isMapValid = ValidateMap(generatedRooms, roomCountToGenerate, levelData);
            iterationsCount++;
        }

        GenerateSpecialRooms(level, levelData);

        return levelData;
    }


    private int GenerateRooms(int roomCountToGenerate, Vector2Int startCoord, LevelData levelData)
    {
        levelData.levelmap[startCoord.x, startCoord.y] = ERoomType.StartRoom;

        Queue<Vector2Int> discoverQueue = new Queue<Vector2Int>();
        discoverQueue.Enqueue(startCoord);

        int generatedRoomCount = 1;
        while (discoverQueue.Count > 0)
        {
            Vector2Int currentCoord = discoverQueue.Dequeue();

            Vector2Int[] neighbourCoord = new Vector2Int[]
            {
                currentCoord + Vector2Int.right,
                currentCoord + Vector2Int.up,
                currentCoord + Vector2Int.left,
                currentCoord + Vector2Int.down
            };

            bool hasGeneratedARoom = false;

            int offset = discoverQueue.Count;
            for (int i = 0; i < neighbourCoord.Length; i++)
            {
                Vector2Int currentNeighbourCoord = neighbourCoord[(offset + i) % 4];

                if (!IsCoordInBounds(currentNeighbourCoord))
                    continue;

                if (generatedRoomCount >= roomCountToGenerate)
                    break;

                if (levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] != ERoomType.Free)
                    continue;

                int randomMax = 2;
                if (i > 2 && !hasGeneratedARoom)
                {
                    randomMax++;
                    if (iterationsCount >= maxIteration / 2  && iterationsCount <= maxIteration * 0.8)
                        randomMax++;
                }
                if (iterationsCount > maxIteration * 0.8)
                    randomMax++;                               

                if (Random.Range(0, randomMax) == 0)
                    continue;

                if (GetNeighbourCount(currentNeighbourCoord, levelData) > 1)
                    continue;

                levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] = ERoomType.Normal;

                generatedRoomCount++;

                discoverQueue.Enqueue(currentNeighbourCoord);

                hasGeneratedARoom = true;
            }

            if (!hasGeneratedARoom)
                levelData.endRooms.Push(currentCoord);
        }

        return generatedRoomCount;
    }

    private bool ValidateMap(int generatedRoomCount, int roomCoundToGenerate, LevelData levelData)
    {
        return generatedRoomCount == roomCoundToGenerate && levelData.endRooms.Count >= 2;
    }

    private void GenerateSpecialRooms(int level, LevelData levelData)
    {
        if (level != levelCount)
        {
            Vector2Int stairsCoord = levelData.endRooms.Pop();
            levelData.levelmap[stairsCoord.x, stairsCoord.y] = ERoomType.Stairs;
        }
        else
        {
            Vector2Int bossCoord = levelData.endRooms.Pop(); ;
            levelData.levelmap[bossCoord.x, bossCoord.y] = ERoomType.Boss;
        }

        Vector2Int shopCoord = levelData.endRooms.Pop();
        levelData.levelmap[shopCoord.x, shopCoord.y] = ERoomType.Shop;

        while (levelData.endRooms.Count > 0)
        {
            Vector2Int itemCoords = levelData.endRooms.Pop();
            levelData.levelmap[itemCoords.x, itemCoords.y] = ERoomType.Item;
        }
    }

    private int GetNeighbourCount(Vector2Int coord, LevelData levelData)
    {
        Vector2Int[] neighbourCoord = new Vector2Int[]
        {
                coord + Vector2Int.right,
                coord + Vector2Int.up,
                coord + Vector2Int.left,
                coord + Vector2Int.down
        };

        int neighbourCound = 0;

        for (int i = 0; i < neighbourCoord.Length; i++)
        {
            Vector2Int currentNeighbourCoord = neighbourCoord[i];

            if (!IsCoordInBounds(currentNeighbourCoord))
                continue;

            if (levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] != ERoomType.Free)
                neighbourCound++;
        }

        return neighbourCound;
    }

    private bool IsCoordInBounds(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }
}
