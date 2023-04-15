using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Rp.GoogleSheet;
using UnityEngine.Networking;
using System.Reflection;
using System.IO;

namespace Rp.CustomEditorWindow.S2S // S2S = Sheet to SO
{
    public class GoogleSheetToSO : EditorWindow
    {
        private ScriptableObject _targetScriptableObject = null;

        private string _sheetKey = "";
        private string _savePath = "";

        private static List<SOVariable> _sheetTypeList = new List<SOVariable>();
        private List<string> _fileNameVariableList = new List<string>();
        private int _fileNameIndex = 0;

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
                _sheetTypeList.Clear();
                if (_targetScriptableObject != null)
                {
                    var test = _targetScriptableObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var item in test)
                    {
                        _sheetTypeList.Add(new SOVariable { type = item.FieldType, name = item.Name, isUsing = true });
                        if (item.FieldType == typeof(string))
                        {
                            _fileNameVariableList.Add(item.Name);
                        }
                    }
                }
            }

            _sheetKey = EditorGUILayout.TextField("Sheet Key", _sheetKey);
            _savePath = EditorGUILayout.TextField("Save Path", _savePath);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            for (int i = 0; i < _sheetTypeList.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();

                string tempType = "";

                if (_sheetTypeList[i].type == typeof(int))
                {
                    tempType = "int";
                }
                else if (_sheetTypeList[i].type == typeof(float))
                {
                    tempType = "float";
                }
                else if (_sheetTypeList[i].type == typeof(string))
                {
                    tempType = "string";
                }
                else if (_sheetTypeList[i].type == typeof(bool))
                {
                    tempType = "bool";
                }
                else
                {
                    _sheetTypeList.RemoveAt(i);
                    continue;
                }

                EditorGUILayout.LabelField(tempType, GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField(_sheetTypeList[i].name, GUILayout.ExpandWidth(true));

                var temp = _sheetTypeList[i];
                temp.isUsing = EditorGUILayout.Toggle(_sheetTypeList[i].isUsing, GUILayout.ExpandWidth(true));
                _sheetTypeList[i] = temp;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            _fileNameIndex = EditorGUILayout.Popup("File Name Variable", _fileNameIndex, _fileNameVariableList.ToArray());

            if (GUILayout.Button("Reset Folder"))
            {
                ResetFolder();
            }

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
            string result2 = result.Replace("\r", "");


            string[] lines = result2.Split('\n');

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] data = lines[i].Split('\t');

                for (int j = 0; j < data.Length; ++j)
                {
                    var temp = ScriptableObject.CreateInstance(_targetScriptableObject.GetType());

                    for (int k = 0; k < _sheetTypeList.Count; ++k)
                    {
                        if (_sheetTypeList[k].isUsing)
                        {
                            temp.GetType().GetField(_sheetTypeList[k].name).SetValue(temp, Convert.ChangeType(data[k], _sheetTypeList[k].type));
                        }
                    }

                    AssetDatabase.CreateAsset(temp, _savePath + "/" + data[_fileNameIndex] + ".asset");
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
        }

        private void OnDisable()
        {
            _sheetTypeList.Clear();
        }

        public struct SOVariable
        {
            public Type type;
            public string name;
            public bool isUsing;
        }

        private void ResetFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + _savePath);

            foreach (var item in dir.GetFiles())
            {
                item.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                di.Delete(true);
            }

            AssetDatabase.Refresh();
        }
    }
}
