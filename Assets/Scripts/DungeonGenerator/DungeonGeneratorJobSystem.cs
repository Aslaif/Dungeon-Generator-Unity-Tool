using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using UnityEngine.Profiling;

public class DungeonGeneratorJobSystem 
{
    private int seed;

    private int width = 9;
    private int height = 8;
    private int levelCount = 1;
    private int minRoomCount = 7;

    Stopwatch stopwatch = new Stopwatch();

    private Dictionary<int, LevelData> levelmaps;

    public DungeonGeneratorJobSystem(int width, int height, int levelCount, int roomCount, Dictionary<int, LevelData> levelmaps, int seed)
    {
        this.width = width;
        this.height = height;
        this.levelCount = levelCount;
        this.minRoomCount = roomCount;
        this.levelmaps = levelmaps;
        this.seed = seed;
    }

    public void StartMultiThread()
    {
        NativeArray<ERoomType>[] maps = new NativeArray<ERoomType>[levelCount];
        NativeArray<Vector2Int>[] trash = new NativeArray<Vector2Int>[levelCount * 2];
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(levelCount, Allocator.TempJob);//veilleicht TempJob
        
        stopwatch.Start();
        Profiler.BeginSample("MultiThreading");

        for (int i = 0; i < levelCount; i++)
        {
            NativeArray<ERoomType> oneDLevelMap = new NativeArray<ERoomType>(width * height, Allocator.TempJob);
            NativeArray<Vector2Int> endRooms = new NativeArray<Vector2Int>(width * height, Allocator.TempJob);
            NativeArray<Vector2Int> discoverQueue = new NativeArray<Vector2Int>(width * height, Allocator.TempJob);

            DungeonGeneratorJob job = new DungeonGeneratorJob
            {
                Width = width,
                Height = height,
                LevelCount = levelCount,
                LevelIndex = i + 1,
                RoomCount = minRoomCount,
                Map = oneDLevelMap,
                DiscoverQueue = discoverQueue,
                EndRooms = endRooms,
                SeedForRandom = seed

            };
            jobs[i] = job.Schedule();
            
            maps[i] = oneDLevelMap;
            
            trash[i * 2] = endRooms;
            trash[i * 2 + 1] = discoverQueue;
        }
        JobHandle.CompleteAll(jobs);

        for (int i = 0; i < maps.Length; i++)
        {
            ERoomType[,] currentMap = new ERoomType[width, height];

            for (int y = 0, q = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, q++)
                {
                    currentMap[x, y] = maps[i][q]; 
                }
            }

            levelmaps.Add(i, new LevelData(currentMap));
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
        Profiler.EndSample();

        foreach (var item in trash)
        {
            item.Dispose();
        }

        foreach (var map in maps)
        {
            map.Dispose();
        }

        jobs.Dispose();
    }
}
