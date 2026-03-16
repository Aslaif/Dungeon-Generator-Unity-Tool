using UnityEditor;
using UnityEngine;
using Assets.Scripts.DungeonGenerator;

namespace Assets.Scripts.Editor
{
    internal class DungeonGeneratorEditor : EditorWindow
    {
        private int tabChoice = 0;

        private DungeonGenerator.DungeonGenerator generator;
        private SerializedObject serializedGenerator;
        private SerializedProperty seed;
        private SerializedProperty multiThreading;
        private SerializedProperty width;
        private SerializedProperty height;
        private SerializedProperty levelCount;
        private SerializedProperty minRoomCount;

        private MapInterpreter interpreter;
        private SerializedObject serializedInterpreter;
        private SerializedProperty roomSize;
        private SerializedProperty roomHeight;
        private SerializedProperty roomsOneDoor;
        private SerializedProperty roomsTwoDoorCorner;
        private SerializedProperty roomsTwoDoorThrough;
        private SerializedProperty roomsThreeDoor;
        private SerializedProperty roomsFourDoors;
        private SerializedProperty endRoomsItem;
        private SerializedProperty endRoomsShop;
        private SerializedProperty endRoomsStairs;
        private SerializedProperty endRoomsBoss;

        private Vector2 scrollPos;

        [MenuItem("Window/Dungeon Generator")]
        public static void ShowWindow()
        {
            GetWindow<DungeonGeneratorEditor>("Dungeon Generator");
        }

        private void OnEnable()
        {
            interpreter = AssetDatabase.LoadAssetAtPath<MapInterpreter>
                ("Assets/ScriptableObjects/MapInterpreter.asset");
            generator = AssetDatabase.LoadAssetAtPath<DungeonGenerator.DungeonGenerator>
                ("Assets/ScriptableObjects/DungeonGenerator.asset");

            if (interpreter != null)
            {
                serializedInterpreter = new SerializedObject(interpreter);
                roomSize = serializedInterpreter.FindProperty("roomSize");
                roomHeight = serializedInterpreter.FindProperty("roomHeight");
                roomsOneDoor = serializedInterpreter.FindProperty("roomsOneDoor");
                roomsTwoDoorCorner = serializedInterpreter.FindProperty("roomsTwoDoorCorner");
                roomsTwoDoorThrough = serializedInterpreter.FindProperty("roomsTwoDoorThrough");
                roomsThreeDoor = serializedInterpreter.FindProperty("roomsThreeDoor");
                roomsFourDoors = serializedInterpreter.FindProperty("roomsFourDoors");
                endRoomsItem = serializedInterpreter.FindProperty("endRoomsItem");
                endRoomsShop = serializedInterpreter.FindProperty("endRoomsShop");
                endRoomsStairs = serializedInterpreter.FindProperty("endRoomsStairs");
                endRoomsBoss = serializedInterpreter.FindProperty("endRoomsBoss");
            }

            if (generator != null)
            {
                serializedGenerator = new SerializedObject(generator);
                seed = serializedGenerator.FindProperty("seed");
                multiThreading = serializedGenerator.FindProperty("multiThreading");
                width = serializedGenerator.FindProperty("width");
                height = serializedGenerator.FindProperty("height");
                levelCount = serializedGenerator.FindProperty("levelCount");
                minRoomCount = serializedGenerator.FindProperty("minRoomCount");
            }
        }

        private void OnGUI()
        {
            tabChoice = GUILayout.Toolbar(tabChoice, new string[] { "DungeonGenerator", "DungeonRooms" });

            if (interpreter == null)
            {
                EditorGUILayout.HelpBox("Interpreter not found.", MessageType.Error);
                return;
            }
            else if (generator == null)
            {
                EditorGUILayout.HelpBox("Generator not found.", MessageType.Error);
                return;
            }

            if (tabChoice == 0)
                OnDungeonGenerator();
            else if (tabChoice == 1)
                OnDungeonRooms();


        }

        private void OnDungeonRooms()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            serializedInterpreter.Update();

            EditorGUILayout.PropertyField(roomSize, new GUIContent("Room Size"), true);
            EditorGUILayout.PropertyField(roomHeight, new GUIContent("Room Height"), true);
            EditorGUILayout.PropertyField(roomsOneDoor, new GUIContent("One Door Rooms"), true);
            EditorGUILayout.PropertyField(roomsTwoDoorCorner, new GUIContent("Two Door Rooms Corner"), true);
            EditorGUILayout.PropertyField(roomsTwoDoorThrough, new GUIContent("Two Door Rooms Through"), true);
            EditorGUILayout.PropertyField(roomsThreeDoor, new GUIContent("Three Door Rooms"), true);
            EditorGUILayout.PropertyField(roomsFourDoors, new GUIContent("Four Door Rooms"), true);
            EditorGUILayout.PropertyField(endRoomsItem, new GUIContent("End Rooms with idem"), true);
            EditorGUILayout.PropertyField(endRoomsShop, new GUIContent("End Rooms with shop"), true);
            EditorGUILayout.PropertyField(endRoomsStairs, new GUIContent("End Rooms with stairs"), true);
            EditorGUILayout.PropertyField(endRoomsBoss, new GUIContent("End Rooms with boss"), true);

            EditorGUILayout.EndScrollView();

            serializedInterpreter.ApplyModifiedProperties();

            if (GUILayout.Button("Apply and generate"))
            {
                ApplyChangesToScriptAndGenerate();
            }
        }

        private void OnDungeonGenerator()
        {
            serializedGenerator.Update();

            EditorGUILayout.PropertyField(seed, new GUIContent("Random Seed"), true);
            EditorGUILayout.PropertyField(multiThreading, new GUIContent("Use Multi Threading"), true);
            EditorGUILayout.PropertyField(width, new GUIContent("Array Width"), true);
            width.intValue = Mathf.Clamp(width.intValue, 8, int.MaxValue);
            EditorGUILayout.PropertyField(height, new GUIContent("Array Height"), true);
            height.intValue = Mathf.Clamp(height.intValue, 8, int.MaxValue);
            EditorGUILayout.PropertyField(levelCount, new GUIContent("Level Count"), true);
            levelCount.intValue = Mathf.Clamp(levelCount.intValue, 1, int.MaxValue);
            EditorGUILayout.PropertyField(minRoomCount, new GUIContent("Minimal Room Count"), true);
            minRoomCount.intValue = Mathf.Clamp(minRoomCount.intValue, 7, int.MaxValue);

            serializedGenerator.ApplyModifiedProperties();
            
            if (GUILayout.Button("Apply and generate"))
            {
                ApplyChangesToScriptAndGenerate();
            }
        }

        private void ApplyChangesToScriptAndGenerate()
        {
            ApplyChangesToScript();

            generator.seed = seed.intValue;
            generator.multiThreading = multiThreading.boolValue;
            generator.width = width.intValue;
            generator.height = height.intValue;
            generator.levelCount = levelCount.intValue;
            generator.minRoomCount = minRoomCount.intValue;

            generator.StartGeneration();
        }

        private void ApplyChangesToScript()
        {
            interpreter.roomsOneDoor.Clear();
            for (int i = 0; i < roomsOneDoor.arraySize; i++)
            {
                SerializedProperty element = roomsOneDoor.GetArrayElementAtIndex(i);
                interpreter.roomsOneDoor.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.roomsTwoDoorCorner.Clear();
            for (int i = 0; i < roomsTwoDoorCorner.arraySize; i++)
            {
                SerializedProperty element = roomsTwoDoorCorner.GetArrayElementAtIndex(i);
                interpreter.roomsTwoDoorCorner.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.roomsTwoDoorThrough.Clear();
            for (int i = 0; i < roomsTwoDoorThrough.arraySize; i++)
            {
                SerializedProperty element = roomsTwoDoorThrough.GetArrayElementAtIndex(i);
                interpreter.roomsTwoDoorThrough.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.roomsThreeDoor.Clear();
            for (int i = 0; i < roomsThreeDoor.arraySize; i++)
            {
                SerializedProperty element = roomsThreeDoor.GetArrayElementAtIndex(i);
                interpreter.roomsThreeDoor.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.roomsFourDoors.Clear();
            for (int i = 0; i < roomsFourDoors.arraySize; i++)
            {
                SerializedProperty element = roomsFourDoors.GetArrayElementAtIndex(i);
                interpreter.roomsFourDoors.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.endRoomsItem.Clear();
            for (int i = 0; i < endRoomsItem.arraySize; i++)
            {
                SerializedProperty element = endRoomsItem.GetArrayElementAtIndex(i);
                interpreter.endRoomsItem.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.endRoomsShop.Clear();
            for (int i = 0; i < endRoomsShop.arraySize; i++)
            {
                SerializedProperty element = endRoomsShop.GetArrayElementAtIndex(i);
                interpreter.endRoomsShop.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.endRoomsStairs.Clear();
            for (int i = 0; i < endRoomsStairs.arraySize; i++)
            {
                SerializedProperty element = endRoomsStairs.GetArrayElementAtIndex(i);
                interpreter.endRoomsStairs.Add((GameObject)element.objectReferenceValue);
            }

            interpreter.endRoomsBoss.Clear();
            for (int i = 0; i < endRoomsBoss.arraySize; i++)
            {
                SerializedProperty element = endRoomsBoss.GetArrayElementAtIndex(i);
                interpreter.endRoomsBoss.Add((GameObject)element.objectReferenceValue);
            }
            
            EditorUtility.SetDirty(interpreter);

            interpreter.roomSize = roomSize.floatValue;
            interpreter.roomHeight = roomHeight.floatValue;
        }
    }
}
