using UnityEditor;
using UnityEngine;

namespace GAutomatorView.Editor
{
    public class MainWindow : SplitableWindow
    {
        public static MainWindow instance;
        
        [MenuItem("Tools/程序向工具/Android远程调试窗")]
        private static void ShowWindow()
        {
            instance = GetWindow<MainWindow>();
            instance.titleContent = new GUIContent("鲁班");

            instance.Init();
            instance.Show();
        }

        private void Init()
        {
            
        }

        private void OnGUI()
        {
            InitSplitEnvironment();
            // EditorGUILayout.BeginVertical();
            // EditorGUILayout.BeginHorizontal();
            // GUILayout.FlexibleSpace();
            // GUILayout.Button("aaaaaa");
            // GUILayout.FlexibleSpace();
            // EditorGUILayout.EndHorizontal();
            
            BeginVerticalSplit();
            {
                GUILayout.Button("453434", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            Split();
            {
                GUILayout.Box("wqwqfsdx", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }            
            EndSplit();
            
            // EditorGUILayout.EndVertical();
        }
    }
}

