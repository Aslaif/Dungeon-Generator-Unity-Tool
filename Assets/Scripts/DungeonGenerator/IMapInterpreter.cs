using System.Collections.Generic;

namespace Assets.Scripts.DungeonGenerator
{
    public interface IMapInterpreter
    {
        public void InterpretMap(Dictionary<int, LevelData> map, int seed);
    }
}
