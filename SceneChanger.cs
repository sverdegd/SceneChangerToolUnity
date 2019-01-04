//SceneChanger Tool by SVerde
//https://github.com/sverdegd
//contact@sverdegd.com

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace SVerdeTools.SceneChanger{

    public class SceneChanger : EditorWindow{

        public static SceneChanger window;

        int index = 0;
        bool onlyInBuildSettings = true;
        bool closeWhenChange = false;
        string[] options;
        string[] paths;
        int scene;
        int totalScenes;
        string searchString;
        List<string> searchOptions = new List<string>();
        string sceneInfo;


        static GUIContent wTitle = new GUIContent();

        [MenuItem("SVerdeTools/Scene Changer %#e")]
        public static void OpenWindow(){
            if (window == null)
            {
                window = (SceneChanger)GetWindow(typeof(SceneChanger));

                wTitle.text = "Scene Changer";
                wTitle.tooltip = "Use this tool to change the scene in the editor";

                window.minSize = new Vector2(400, 190);
                window.titleContent = wTitle;
            }

            window.Show();
        }

        void OnEnable()
        {
            onlyInBuildSettings = EditorPrefs.GetBool("SceneChanger.OnlyInBuildSettings");
            closeWhenChange = EditorPrefs.GetBool("SceneChanger.CloseWhenChange");
            Reload();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Choose an scene and click on \"Open scene\" to load it.", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            onlyInBuildSettings = EditorGUILayout.ToggleLeft("Show only scenes that are in Build Settings", onlyInBuildSettings);
            if (EditorGUI.EndChangeCheck()) {
                EditorPrefs.SetBool("SceneChanger.OnlyInBuildSettings", onlyInBuildSettings);
                Reload();
            }
            EditorGUI.BeginChangeCheck();
            closeWhenChange = EditorGUILayout.ToggleLeft("Close the window after switching scenes", closeWhenChange);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("SceneChanger.CloseWhenChange", closeWhenChange);
            }
            
            EditorGUILayout.Space();

            if (totalScenes != 0)
            {
                #region Search
                GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
                EditorGUI.BeginChangeCheck();
                searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (EditorGUI.EndChangeCheck())
                {
                    index = 0;
                }
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    searchString = "";
                    index = 0;
                    GUI.FocusControl(null);
                }
                GUILayout.EndHorizontal();

                searchOptions.Clear();
                if (searchString != "")
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        if ((options[i].ToLower()).Contains(searchString.ToLower()))
                        {
                            searchOptions.Add(options[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < options.Length; i++)
                    {
                        searchOptions.Add(options[i]);
                    }
                }

                #endregion /Search

                if (searchOptions.Count != 0) {
                    index = EditorGUILayout.Popup(index, searchOptions.ToArray());

                    GUI.color = Color.green;
                    if (GUILayout.Button("Open Scene"))
                    {
                        ChangeScene();
                        if (closeWhenChange) Close();
                    }
                    GUI.color = Color.white;
                    EditorGUILayout.Space();
                }
                else
                {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField("No search results", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();

                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<< Open Previous Scene"))
                {
                    if (scene - 1 >= 0)
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        EditorSceneManager.OpenScene(paths[scene - 1]);
                        Reload();
                        Print("Previous scene loaded, " + options[scene - 1]);
                        if (closeWhenChange) Close();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "There is no previous scene ", "Ok");
                    }
                }
                if (GUILayout.Button("Open Next Scene >>"))
                {
                    if (scene + 1 < totalScenes)
                    {
                        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        EditorSceneManager.OpenScene(paths[scene + 1]);
                        Reload();
                        Print("Next scene loaded, " + options[scene + 1]);
                        if (closeWhenChange) Close();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "There is no next scene ", "Ok");
                    }

                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Reload Scene List"))
                {
                    Reload();
                }

                if (GUILayout.Button("Open Build Settings"))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                }

                EditorGUILayout.LabelField(sceneInfo, EditorStyles.miniLabel);

            }
            else
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("There are no scenes");
                GUI.color = Color.green;
                if (GUILayout.Button("Open Build Settings"))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                }
                GUI.color = Color.white;
                if (GUILayout.Button("Reload"))
                {
                    Reload();
                }
                EditorGUILayout.EndVertical();
            }
        }

        void ChangeScene()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            int aux = 0;
           
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == searchOptions[index])
                {
                    aux = i;
                    break;
                }
            }
             
            EditorSceneManager.OpenScene(paths[aux]);
            Reload();
            Print("Loaded scene, " + options[aux]);
        }

        void Reload()
        {
            index = 0;
            searchString = "";
           
            if (onlyInBuildSettings)
            {
                SetupOnlyBuild();
            }
            else
            {
                SetupAll();
            }
        }

        void SetupOnlyBuild()
        {
            totalScenes = EditorSceneManager.sceneCountInBuildSettings;
            
            if(totalScenes == 0)
            {
                return;
            }

            options = new string[totalScenes];
            paths = new string[totalScenes];

            scene = -1;
            for (int i = 0; i < totalScenes; i++)
            {
                paths[i] = SceneUtility.GetScenePathByBuildIndex(i);
                options[i] = GetName(paths[i]);

                if (GetName(EditorSceneManager.GetActiveScene().path) == options[i])
                {
                    scene = i;
                }
            }

            if (scene == -1)
            {
                sceneInfo = "Current scene: \"" + GetName(EditorSceneManager.GetActiveScene().path) + "\" (The loaded scene is not in Build Settings/" + totalScenes.ToString() + ")";
            }
            else
            {
                UpdateSceneInfo();
            }
        }


        void SetupAll()
        {
            string[] guids;
            guids = AssetDatabase.FindAssets("t: Scene");

            totalScenes = guids.Length;

            if (totalScenes == 0)
            {
                return;
            }

            options = new string[totalScenes];
            paths = new string[totalScenes];

            for (int i = 0; i < guids.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                options[i] = GetName(paths[i]);
               
                if (GetName(EditorSceneManager.GetActiveScene().path) == options[i])
                {
                    scene = i;
                }
            }

            UpdateSceneInfo();

        }

        string GetName(string path)
        {
            return path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
        }

        void UpdateSceneInfo(){
            sceneInfo = "Current scene: \"" + options[scene] + "\" (" + (scene + 1).ToString() + "/" + totalScenes.ToString() + ")";
        }

        void Print(string msg)
        {
            Debug.Log("<color=green>SceneChanger: </color>" + msg);
        }
    }
}