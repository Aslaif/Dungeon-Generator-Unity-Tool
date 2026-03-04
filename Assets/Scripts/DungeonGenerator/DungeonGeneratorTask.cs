//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine;
//using Unity.Jobs;
//using Unity.Collections;
//using static Cinemachine.DocumentationSortingAttribute;
//using Assets.Scripts.DungeonGenerator;
//using System.Threading.Tasks;
//using System;
//using Random = System.Random;

//public partial class DungeonGeneratorTask : MonoBehaviour
//{
//    [SerializeField] private MonoMapInterpreter monoMapInterpreter;
//    private IMapInterpreter interpreter;

//    [SerializeField] private int width = 9;
//    [SerializeField] private int height = 8;
//    [SerializeField] private int levelCount = 1;

//    private Dictionary<int, LevelData> levelmaps = new Dictionary<int, LevelData>();

//    //List<Task> tasks = new List<Task>();
//    new Task<LevelData>[] tasks;

//    private void Awake()
//    {
//        tasks = new Task<LevelData>[levelCount];

//        if (monoMapInterpreter != null)
//            interpreter = monoMapInterpreter;

//        //for (int i = 1; i > levelCount; i++)
//        //{
//        //    levelmaps.Add(i, GenerateLevelMap(i));
//        //}

//        for (int i = 0; i < levelCount; i++)
//        {
//            var levelData = new LevelData(width, height);
//            levelmaps.Add(i, levelData);
//            tasks[i] = Task.Run(() => GenerateLevelMap(i));
//        }
//        Task.WaitAll(tasks);

//        //for (int i = 1; i < levelCount; i++)
//        //{
//        //    LevelData levelData = new LevelData(width, height);
//        //    tasks.Add(Task.Factory.StartNew(() => levelData = GenerateLevelMap(i)));
//        //}
//        //Task.WaitAll(tasks.ToArray());  // wie bekomme ich ein rueckgabewert? await

//        Debug.Log(tasks);

//        for (int i = 1; i < levelmaps.Count + 1; i++)  // debug
//        {
//            var levelData = levelmaps[i];

//            Debug.Log(levelData.levelmap);
//        }
//    }

//    private void Start()
//    {
//        interpreter.InterpretMap(levelmaps);
//    }

//    private async Task<LevelData> GenerateLevelMap(int level)
//    {
//        Random rnd = new Random();
//        LevelData levelData = new LevelData(width, height);
//        int roomCountToGenerate = (int)(rnd.Next(0, 2) + 5 + level * 2.6f);
//        Vector2Int startCoord = new Vector2Int(width, height) / 2;

//        bool isMapValid = false;
//        int maxIteration = 20;
//        int iterationsCound = 0;
//        while (!isMapValid && iterationsCound < maxIteration)
//        {
//            int generatedRooms = GenerateRooms(roomCountToGenerate, startCoord, levelData);
//            isMapValid = ValidateMap(generatedRooms, roomCountToGenerate, levelData);
//            iterationsCound++;
//        }

//        GenerateSpecialRooms(level, levelData);

//        return levelData;
//    }

//    private int GenerateRooms(int roomCountToGenerate, Vector2Int startCoord, LevelData levelData)
//    {
//        levelData.levelmap[startCoord.x, startCoord.y] = ERoomType.normal;

//        Queue<Vector2Int> discoverQueue = new Queue<Vector2Int>();
//        discoverQueue.Enqueue(startCoord);

//        int generatedRoomCount = 1;
//        while (discoverQueue.Count > 0)
//        {
//            Random rnd = new Random();

//            Vector2Int currentCoord = discoverQueue.Dequeue();

//            Vector2Int[] neighbourCoord = new Vector2Int[]
//            {
//                currentCoord + Vector2Int.right,
//                currentCoord + Vector2Int.up,
//                currentCoord + Vector2Int.left,
//                currentCoord + Vector2Int.down
//            };

//            bool hasGeneratedARoom = false;

//            for (int i = 0; i < neighbourCoord.Length; i++)
//            {
//                Vector2Int currentNeighbourCoord = neighbourCoord[i];

//                if (!IsCoordInBounds(currentNeighbourCoord))
//                    continue;

//                if (generatedRoomCount >= roomCountToGenerate)
//                    break;

//                if (levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] != ERoomType.free)
//                    continue;

//                if (rnd.Next(0, 2) == 0)
//                    continue;

//                if (GetNeighbourCount(currentNeighbourCoord, levelData) > 1)
//                    continue;

//                levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] = ERoomType.normal;

//                generatedRoomCount++;

//                discoverQueue.Enqueue(currentNeighbourCoord);

//                hasGeneratedARoom = true;
//            }

//            //if (!hasGeneratedARoom)
//                //levelData.endRooms.Add(currentCoord);
//        }

//        return generatedRoomCount;
//    }

//    private bool ValidateMap(int generatedRoomCount, int roomCoundToGenerate, LevelData levelData)
//    {
//        return generatedRoomCount == roomCoundToGenerate && levelData.endRooms.Count >= 2;
//    }

//    private void GenerateSpecialRooms(int level, LevelData levelData)
//    {
//        int lastIndex = levelData.endRooms.Count - 1;

//        if (level != levelCount)
//        {
//            Vector2Int stairsCoord = levelData.endRooms[lastIndex];
//            levelData.levelmap[stairsCoord.x, stairsCoord.y] = ERoomType.stairs;
//            levelData.endRooms.RemoveAt(lastIndex);
//        }
//        else
//        {
//            Vector2Int bossCoord = levelData.endRooms[lastIndex];
//            levelData.levelmap[bossCoord.x, bossCoord.y] = ERoomType.boss;
//            levelData.endRooms.RemoveAt(lastIndex);
//        }

//        lastIndex--;
//        Vector2Int shopCoord = levelData.endRooms[lastIndex];
//        levelData.levelmap[shopCoord.x, shopCoord.y] = ERoomType.shop;
//        levelData.endRooms.RemoveAt(lastIndex);

//        for (int i = levelData.endRooms.Count - 1; i > 0; i--)
//        {
//            lastIndex--;
//            Vector2Int itemCoords = levelData.endRooms[lastIndex];
//            levelData.levelmap[itemCoords.x, itemCoords.y] = ERoomType.item;
//            levelData.endRooms.RemoveAt(lastIndex);
//            // eventueller denkfehler. keonnte in die forschleife gehen ohne weiteren endroom zu haben.
//        }


//    }

//    private int GetNeighbourCount(Vector2Int coord, LevelData levelData)
//    {
//        Vector2Int[] neighbourCoord = new Vector2Int[]
//        {
//                coord + Vector2Int.right,
//                coord + Vector2Int.up,
//                coord + Vector2Int.left,
//                coord + Vector2Int.down
//        };

//        int neighbourCound = 0;

//        for (int i = 0; i < neighbourCoord.Length; i++)
//        {
//            Vector2Int currentNeighbourCoord = neighbourCoord[i];

//            if (!IsCoordInBounds(currentNeighbourCoord))
//                continue;

//            if (levelData.levelmap[currentNeighbourCoord.x, currentNeighbourCoord.y] != ERoomType.free)
//                neighbourCound++;
//        }

//        return neighbourCound;
//    }

//    private bool IsCoordInBounds(Vector2Int coord)
//    {
//        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
//    }
//}
