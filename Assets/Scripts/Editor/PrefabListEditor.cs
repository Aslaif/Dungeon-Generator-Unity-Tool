using Assets.Scripts.DungeonGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(PrefabList))]
    public class PrefabListEditor : UnityEditor.Editor
    {
        private SerializedProperty prefabList;

        private void OnEnable()
        {
            // Die 'prefabs'-Liste aus dem Skript holen
            prefabList = serializedObject.FindProperty("Prefabs");
        }

        public override void OnInspectorGUI()
        {
            // Aktualisiert den SerializedObject
            serializedObject.Update();

            // Zeichnet die Liste der Prefabs
            EditorGUILayout.PropertyField(prefabList, new GUIContent("Prefab List"), true);

            // Fügt einen Button hinzu, um ein neues Prefab zur Liste hinzuzufügen
            if (GUILayout.Button("Add Prefab"))
            {
                prefabList.arraySize++;
            }

            // Anwendung der Änderungen auf das SerializedObject
            serializedObject.ApplyModifiedProperties();
        }
    }
}
