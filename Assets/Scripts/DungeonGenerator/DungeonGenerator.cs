using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.DungeonGenerator
{
    [RequireComponent(typeof(MapInterpreter))]
    [CreateAssetMenu(fileName = "DungeonGenerator", menuName = "DungeonGenerator/DungeonGenerator")]
    public class DungeonGenerator : ScriptableObject
    {
        public int seed;
        public bool multiThreading;

        [Min(8)] public int width = 9;
        [Min(8)] public int height = 8;
        [Min(1)] public int levelCount = 1;
        [Min(7)] public int minRoomCount = 7;

        Dictionary<int, LevelData> levelmaps;

        DungeonGeneratorSingleThread singleThread;
        DungeonGeneratorJobSystem multiThread;

        private MapInterpreter mapInterpreter;
        private IMapInterpreter interpreter;

        private void OnValidate()
        {
            if (minRoomCount > (int)(width + height - levelCount * 2))
            {
                int value = (int)(width + height - levelCount * 2);
                minRoomCount = Mathf.Max(7, value);
            }
        }

        public void StartGeneration ()
        {
            mapInterpreter = AssetDatabase.LoadAssetAtPath<MapInterpreter>
                ("Assets/ScriptableObjects/MapInterpreter.asset");
            Init();

            interpreter.InterpretMap(levelmaps, seed);
        }

        public void Init()
        {
            levelmaps = new Dictionary<int, LevelData>();
            seed = seed == 0 ? (int)System.DateTime.Now.Ticks : seed;

            if (multiThreading)
                multiThread = new DungeonGeneratorJobSystem(width, height, levelCount, minRoomCount, levelmaps, seed);
            else 
                singleThread = new DungeonGeneratorSingleThread(width, height, levelCount, minRoomCount, levelmaps, seed);
            

            if (mapInterpreter != null)
                interpreter = mapInterpreter;

            if (multiThreading)
                multiThread.StartMultiThread();
            else
                singleThread.StartSingleThread();

            Debug.Log(GetLevelLog());
        }

        private string GetLevelLog()
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < height; y++)
            {
                sb.Append("|");
                for (int x = 0; x < width; x++)
                {
                    var value = (int)levelmaps[0].levelmap[x, y];

                    if (value >= 0)
                        sb.Append(" ");

                    sb.Append(value.ToString() + "|");
                }
                sb.Append("\n");
            }

            return sb.ToString() + $"\n {levelmaps.Count} levels generated.";
        }
    }
}
