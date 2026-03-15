using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DungeonGenerator
{
    public class MonoMapInterpreter : MonoBehaviour, IMapInterpreter
    {
        public float roomSize = 10f;
        public float roomHeight = 2f;
        public List<GameObject> roomsOneDoor = new List<GameObject>();
        public List<GameObject> roomsTwoDoorCorner = new List<GameObject>();
        public List<GameObject> roomsTwoDoorThrough = new List<GameObject>();
        public List<GameObject> roomsThreeDoor = new List<GameObject>();
        public List<GameObject> roomsFourDoors = new List<GameObject>();
        public List<GameObject> endRoomsItem = new List<GameObject>();
        public List<GameObject> endRoomsShop = new List<GameObject>();
        public List<GameObject> endRoomsStairs = new List<GameObject>();
        public List<GameObject> endRoomsBoss = new List<GameObject>();

        private ERoomType[,] currentMap;
        private int currentMapWidth;
        private int currentMapHeight;

        Transform currentLevelParent;


        public void InterpretMap(Dictionary<int, LevelData> map, int seed)
        {
            seed = seed == 0 ? (int)System.DateTime.Now.Ticks : seed;
            Random.InitState(seed);

            Vector2Int lastStairsPosition = Vector2Int.zero;

            for (int i = 0; i < map.Count; i++)
            {
                GameObject newLevel = new GameObject($"Level{i + 1}");
                currentLevelParent = newLevel.transform;
                var levelData = map[i];
                lastStairsPosition = InterpretLevel(i, levelData, lastStairsPosition);
            }
        }

        private Vector2Int InterpretLevel(int i, LevelData levelData, Vector2Int lastStairsPosition)
        {
            Vector2Int startRoomPosition = Vector2Int.zero;

            bool startRoomFound = false;
            for (int x = 0; x < currentMapWidth; x++)
            {
                for (int y = 0; y < currentMapHeight; y++)
                {
                    if (levelData.levelmap[x, y] == ERoomType.StartRoom)
                    {
                        startRoomPosition = new Vector2Int(x, y);
                        startRoomFound = true;
                        break;
                    }
                }
                if (startRoomFound)
                    break;
            }

            Vector2Int offset = lastStairsPosition - startRoomPosition;

            currentMap = levelData.levelmap;
            currentMapWidth = levelData.levelmap.GetLength(0);
            currentMapHeight = levelData.levelmap.GetLength(1);

            Vector2Int currentStarsPosition = Vector2Int.zero;

            for (int y = 0; y < currentMapHeight; y++)
            {
                for (int x = 0; x < currentMapWidth; x++)
                {
                    if (levelData.levelmap[x, y] == ERoomType.Free)
                        continue;
                    InterpretRoom(i, levelData, y, x, offset);
                    if (levelData.levelmap[x, y] == ERoomType.Stairs)
                        currentStarsPosition = new Vector2Int(x, y);
                }
            }

            return currentStarsPosition + offset;
        }

        private void InterpretRoom(int i, LevelData levelData, int y, int x, Vector2Int offset)
        {
            bool[] hasRoomInDirection = new bool[4];
            int neighbourCount = GetNeighbourCountAndDirections(ref hasRoomInDirection, new Vector2Int(x, y));

            GameObject prefabToSpawn = roomsFourDoors[Random.Range(0, roomsFourDoors.Count)];
            float prefabSpawnRotation = 0f;

            switch (neighbourCount)
            {
                case 1:
                    {
                        if (levelData.levelmap[x, y] == ERoomType.Item)
                            prefabToSpawn = endRoomsItem[Random.Range(0, endRoomsItem.Count)];
                        else if (levelData.levelmap[x, y] == ERoomType.Shop)
                            prefabToSpawn = endRoomsShop[Random.Range(0, endRoomsShop.Count)];
                        else if (levelData.levelmap[x, y] == ERoomType.Stairs)
                            prefabToSpawn = endRoomsStairs[Random.Range(0, endRoomsStairs.Count)];
                        else if (levelData.levelmap[x, y] == ERoomType.Boss)
                            prefabToSpawn = endRoomsBoss[Random.Range(0, endRoomsBoss.Count)];
                        else
                            prefabToSpawn = roomsOneDoor[Random.Range(0, roomsFourDoors.Count)];

                        if (hasRoomInDirection[1])
                            prefabSpawnRotation = 90f;
                        else if (hasRoomInDirection[2])
                            prefabSpawnRotation = 180f;
                        else if (!hasRoomInDirection[0])
                            prefabSpawnRotation = 270f;

                        break;
                    }
                case 2:
                    {
                        prefabToSpawn = roomsTwoDoorCorner[Random.Range(0, roomsTwoDoorCorner.Count)];

                        if (hasRoomInDirection[0] && hasRoomInDirection[2])
                            prefabToSpawn = roomsTwoDoorThrough[Random.Range(0, roomsTwoDoorThrough.Count)];
                        else if (hasRoomInDirection[1] && hasRoomInDirection[3])
                        {
                            prefabToSpawn = roomsTwoDoorThrough[Random.Range(0, roomsTwoDoorThrough.Count)];
                            prefabSpawnRotation = 90f;
                        }
                        else if (hasRoomInDirection[1] && hasRoomInDirection[2])
                            prefabSpawnRotation = 180;
                        else if (hasRoomInDirection[2] && hasRoomInDirection[3])
                            prefabSpawnRotation = 270;
                        else if (hasRoomInDirection[0] && hasRoomInDirection[1])
                            prefabSpawnRotation = 90;


                        break;
                    }
                case 3:
                    {
                        prefabToSpawn = roomsThreeDoor[Random.Range(0, roomsThreeDoor.Count)];

                        if (!hasRoomInDirection[0])
                            prefabSpawnRotation = 270;
                        else if (!hasRoomInDirection[2])
                            prefabSpawnRotation = 90f;
                        else if (!hasRoomInDirection[3])
                            prefabSpawnRotation = 180f;

                        break;
                    }
            }

            GameObject newRoom = Instantiate(prefabToSpawn, currentLevelParent);
            if (levelData.levelmap[x, y] == ERoomType.StartRoom)
                newRoom.name = $"StartRoom{i + 1}";
            else if (levelData.levelmap[x, y] == ERoomType.Stairs)
                newRoom.name = $"Stairs{i + 1}";
            newRoom.transform.localRotation = Quaternion.Euler(Vector3.up * prefabSpawnRotation);
            newRoom.transform.localPosition = new Vector3(x * roomSize + offset.x * roomSize, -((i + 1) * roomHeight), y * roomSize + offset.y * roomSize);
        }

        private int GetNeighbourCountAndDirections(ref bool[] directionsArray, Vector2Int coord)
        {
            if (directionsArray == null || directionsArray.Length != 4)
                return 0;

            Vector2Int[] neighbourCoord = new Vector2Int[]
            {
                coord + Vector2Int.up,
                coord + Vector2Int.right,
                coord + Vector2Int.down,
                coord + Vector2Int.left
            };

            int neighbourCound = 0;

            for (int i = 0; i < neighbourCoord.Length; i++)
            {
                Vector2Int currentNeighbourCoord = neighbourCoord[i];
                directionsArray[i] = false;                         // warum? ist doch schon false


                if (!IsCoordInBounds(currentNeighbourCoord))
                    continue;

                if (currentMap[currentNeighbourCoord.x, currentNeighbourCoord.y] != ERoomType.Free)
                {
                    directionsArray[i] = true;
                    neighbourCound++;
                }
            }

            return neighbourCound;
        }

        private bool IsCoordInBounds(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < currentMapWidth && coord.y >= 0 && coord.y < currentMapHeight;
        }
    }
}
