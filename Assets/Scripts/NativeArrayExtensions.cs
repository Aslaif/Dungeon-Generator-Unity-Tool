using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public static class NativeArrayExtensions
    {
        public static T GetAtPosition<T>(this NativeArray<T> source, int x, int y, int gridWidth) where T : struct
        {
            return source[y * gridWidth + x];
        }

        public static void SetAtPosition<T>(this NativeArray<T> source, T value, int x, int y, int gridWidth) where T : struct
        {
            source[y * gridWidth + x] = value;
        }

        public static T GetAtPosition<T>(this NativeArray<T> source, Vector2Int postion , int gridWidth) where T : struct
        {
            return source.GetAtPosition(postion.x, postion.y, gridWidth);
        }

        public static void SetAtPosition<T>(this NativeArray<T> source, T value, Vector2Int postion, int gridWidth) where T : struct
        {
            source.SetAtPosition(value, postion.x, postion.y, gridWidth);
        }
    }
}
