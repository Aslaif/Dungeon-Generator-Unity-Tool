using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Assets.Scripts;
using Unity.Burst;

[BurstCompile]
public struct DungeonGeneratorJob : IJob
{
    public int Width;
    public int Height;
    public int LevelCount;
    public int LevelIndex;
    public int RoomCount;
    public NativeArray<ERoomType> Map;
    public NativeArray<Vector2Int> EndRooms;
    public NativeArray<Vector2Int> DiscoverQueue;
    public int SeedForRandom;

    private int currentEndRoom;

    private Unity.Mathematics.Random random;

    private int maxIteration;
    private int iterationsCound;

    public void Execute()
    {
        random = new Unity.Mathematics.Random();
        random.InitState((uint)SeedForRandom);
        maxIteration = 500;
        iterationsCound = 0;

        int roomCountToGenerate = (int)Unity.Mathematics.math.clamp((random.NextInt(0, 2) + RoomCount - 2 + LevelIndex * 2.6f), RoomCount, Width * Height / 5);
        Vector2Int startCoord = new Vector2Int(Width, Height) / 2;

        bool isMapValid = false;
        while (!isMapValid && iterationsCound < maxIteration)
        {
            for (int y = 0, q = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++, q++)
                {
                    Map[q] = ERoomType.Free;
                }
            }

            for (int y = 0, q = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++, q++)
                {
                    EndRooms[q] = Vector2Int.zero;
                }
            }

            currentEndRoom = 0;
            int generatedRooms = GenerateRooms(roomCountToGenerate, startCoord);
            isMapValid = generatedRooms == roomCountToGenerate && currentEndRoom >= 2;
            iterationsCound++;
        }

        GenerateSpecialRooms();
    }

    private int GenerateRooms(int roomCountToGenerate, Vector2Int startCoord)
    {
        Map.SetAtPosition(ERoomType.StartRoom, startCoord, Width);
        int lastQueuePosition = 1;
        int firstQueuePostion = 0;

        DiscoverQueue[firstQueuePostion] = startCoord;
        int generatedRoomCount = 1;

        while (lastQueuePosition > firstQueuePostion)
        {
            Vector2Int currentCoord = DiscoverQueue[firstQueuePostion];
            firstQueuePostion++;

            bool hasGeneratedARoom = false;

            for (int i = 0; i < 4; i++)
            {
                Vector2Int currentNeighbourCoord = Vector2Int.zero;

                int offset = lastQueuePosition - firstQueuePostion;
                switch ((offset + i) % 4)
                {
                    case 0:
                        currentNeighbourCoord = currentCoord + Vector2Int.right;
                        break;
                    case 1:
                        currentNeighbourCoord = currentCoord + Vector2Int.up;
                        break;
                    case 2:
                        currentNeighbourCoord = currentCoord + Vector2Int.left;
                        break;
                    case 3:
                        currentNeighbourCoord = currentCoord + Vector2Int.down;
                        break;
                    default:
                        break;
                }

                if (!IsCoordInBounds(currentNeighbourCoord))
                    continue;

                if (generatedRoomCount >= roomCountToGenerate)
                    break;

                if (Map.GetAtPosition(currentNeighbourCoord, Width) != ERoomType.Free)
                    continue;

                int randomMax = 2;
                if (i > 2 && !hasGeneratedARoom)
                {
                    randomMax++;
                    if (iterationsCound >= maxIteration / 2 && iterationsCound <= maxIteration * 0.8)
                        randomMax++;
                }
                if (iterationsCound > maxIteration * 0.8)
                    randomMax++;


                if (random.NextInt(0, randomMax) == 0)
                    continue;

                if (GetNeighbourCount(currentNeighbourCoord) > 1)
                    continue;

                Map.SetAtPosition(ERoomType.Normal, currentNeighbourCoord, Width);

                generatedRoomCount++;

                DiscoverQueue[lastQueuePosition] = currentNeighbourCoord;
                lastQueuePosition++;

                hasGeneratedARoom = true;
            }

            if (!hasGeneratedARoom)
            {
                EndRooms[currentEndRoom] = currentCoord;
                currentEndRoom++;
            }
        }

        return generatedRoomCount;
    }

    private bool IsCoordInBounds(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < Width && coord.y >= 0 && coord.y < Height;
    }

    private int GetNeighbourCount(Vector2Int coord)
    {
        int neighbourCount = 0;

        for (int i = 0; i < 4; i++)
        {
            Vector2Int currentNeighbourCoord = Vector2Int.zero;

            switch (i)
            {
                case 0:
                    currentNeighbourCoord = coord + Vector2Int.right;
                    break;
                case 1:
                    currentNeighbourCoord = coord + Vector2Int.up;
                    break;
                case 2:
                    currentNeighbourCoord = coord + Vector2Int.left;
                    break;
                case 3:
                    currentNeighbourCoord = coord + Vector2Int.down;
                    break;
                default:
                    break;
            }

            if (!IsCoordInBounds(currentNeighbourCoord))
                continue;

            if (Map.GetAtPosition(currentNeighbourCoord, Width) != ERoomType.Free)
                neighbourCount++;
        }

        return neighbourCount;
    }

    private bool ValidateMap(int generatedRoomCount, int roomCountToGenerate)
    {
        Debug.Log($"{generatedRoomCount}:{roomCountToGenerate}:{currentEndRoom}");
        return generatedRoomCount == roomCountToGenerate && currentEndRoom >= 2;
    }

    private void GenerateSpecialRooms()
    {
        currentEndRoom--;

        if (LevelIndex != LevelCount)
        {
            Vector2Int stairsCoord = EndRooms[currentEndRoom];
            Map.SetAtPosition(ERoomType.Stairs, stairsCoord, Width);
        }
        else
        {
            Vector2Int bossCoord = EndRooms[currentEndRoom];
            Map.SetAtPosition(ERoomType.Boss, bossCoord, Width);
        }
        currentEndRoom--;

        Vector2Int shopCoord = EndRooms[currentEndRoom];
        Map.SetAtPosition(ERoomType.Shop, shopCoord, Width);
        currentEndRoom--;

        while (currentEndRoom >= 0)
        {
            Vector2Int itemCoords = EndRooms[currentEndRoom];
            Map.SetAtPosition(ERoomType.Item, itemCoords, Width);
            currentEndRoom--;
        }
    }
}



