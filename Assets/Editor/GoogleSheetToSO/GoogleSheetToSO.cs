using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rp.GoogleSheet;
using UnityEngine.Networking;
using System.Reflection;

namespace Rp.CustomEditorWindow.S2S // S2S = Sheet to SO
{
    public class GoogleSheetToSO : EditorWindow
    {
        private ScriptableObject _targetScriptableObject = null;

        private string _sheetKey = "";
        private string _savePath = "";

        private static List<SOVariable> _sheetTypeDictionary = new List<SOVariable>();

        private Vector2 _scrollPos = Vector2.zero;


        [MenuItem("Window/GoogleSheetToSO")]
        public static void Init()
        {
            GoogleSheetToSO window = (GoogleSheetToSO)EditorWindow.GetWindow(typeof(GoogleSheetToSO));

            window.titleContent = new GUIContent("GoogleSheetToSO");

            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Google Sheet to SO", EditorStyles.boldLabel);

            _targetScriptableObject = (ScriptableObject)EditorGUILayout.ObjectField("SO Template", _targetScriptableObject, typeof(ScriptableObject), false);

            if (GUI.changed)
            {
                _sheetTypeDictionary.Clear();
                if (_targetScriptableObject != null)
                {
                    var test = _targetScriptableObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var item in test)
                    {
                        _sheetTypeDictionary.Add(new SOVariable { type = item.FieldType, name = item.Name, isUsing = true });
                    }
                }
            }

            _sheetKey = EditorGUILayout.TextField("Sheet Key", _sheetKey);
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _sheetTypeDictionary.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                string tempType = "";

                if (_sheetTypeDictionary[i].type == typeof(int))
                {
                    tempType = "int";
                }
                else if (_sheetTypeDictionary[i].type == typeof(float))
                {
                    tempType = "float";
                }
                else if (_sheetTypeDictionary[i].type == typeof(string))
                {
                    tempType = "string";
                }
                else if (_sheetTypeDictionary[i].type == typeof(bool))
                {
                    tempType = "bool";
                }

                EditorGUILayout.LabelField(tempType, GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField(_sheetTypeDictionary[i].name, GUILayout.ExpandWidth(true));

                var temp = _sheetTypeDictionary[i];
                temp.isUsing = EditorGUILayout.Toggle(_sheetTypeDictionary[i].isUsing, GUILayout.ExpandWidth(true));
                _sheetTypeDictionary[i] = temp;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Get Sheet Data"))
            {
                GetDataAndSave();
            }
        }

        private void GetDataAndSave()
        {
            UnityWebRequest www = UnityWebRequest.Get(GoogleSheetUtil.GetSheetLink(_sheetKey));
            www.SendWebRequest();

            while (!www.isDone) { }

            string result = www.downloadHandler.text;
            Debug.Log(result);
        }

        private void OnDisable()
        {
            _sheetTypeDictionary.Clear();
        }

        public struct SOVariable
        {
            public Type type;
            public string name;
            public bool isUsing;
        }
    }
}
