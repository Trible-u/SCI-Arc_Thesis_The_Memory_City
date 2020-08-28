using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

namespace MTAssets.EasyMeshCombiner.Editor
{
    public class MeshCombinerTool : EditorWindow
    {

        /*
         This class is responsible for the functioning of the "Easy Mesh Combiner" component, and all its functions.
        */
        /*
         * The Easy Mesh Combiner was developed by Marcos Tomaz in 2019.
         * Need help? Contact me (mtassets@windsoft.xyz)
         */

        //Private constants
        private int MAX_VERTICES_FOR_16BITS_MESH = 50000; //Not change this

        //Variables of window
        private bool isWindowOnFocus = false;

        //Variables of script
        public class GameObjectWithMesh
        {
            //Class that store a gameobject that contains mesh filter or mesh renderer
            public GameObject gameObject;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;

            public GameObjectWithMesh(GameObject gameObject, MeshFilter meshFilter, MeshRenderer meshRenderer)
            {
                this.gameObject = gameObject;
                this.meshFilter = meshFilter;
                this.meshRenderer = meshRenderer;
            }
        }
        public class StatisticsOfMerge
        {
            //Class that store the stats of merge
            public int totalVertex;
            public int meshesAndSubmeshesCount;
            public int materialsCount;
            public int drawCallsAproximate;
            public float optimizationRate;
        }
        public class SelectedItem
        {
            //Class that stores a selected item, to show in scene gui
            public GameObject gameObject;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
            public bool isValidGameObject;

            public SelectedItem(GameObject gameObject, MeshFilter meshFilter, MeshRenderer meshRenderer, bool isValidGameObject)
            {
                this.gameObject = gameObject;
                this.meshFilter = meshFilter;
                this.meshRenderer = meshRenderer;
                this.isValidGameObject = isValidGameObject;
            }
        }
        private static MeshCombinerPreferences meshCombinerPreferences;
        private bool preferencesLoadedOnInspectorUpdate = false;
        private bool mergeIsDone = false;

        //Variables of UI
        public class LogOfMerge
        {
            public MessageType logType;
            public string message;

            public LogOfMerge(MessageType logType, string message)
            {
                this.logType = logType;
                this.message = message;
            }
        }
        private Vector2 scrollPosPreferences;
        private Vector2 scrollPosLogs;
        private int lastQuantityOfLogs = 0;
        private Vector2 scrollPosStats;
        private GameObject[] gameObjectsSelected = new GameObject[0];
        private List<LogOfMerge> logsOfMerge = new List<LogOfMerge>();
        private StatisticsOfMerge statisticsBeforeMerge;
        private StatisticsOfMerge statisticsAfterMerge;
        private List<GameObjectWithMesh> validsGameObjects = new List<GameObjectWithMesh>();
        private int invalidsGameObjectsCount = 0;
        private List<SelectedItem> selectedGameObjectsToShow = new List<SelectedItem>();
        private int targetIndexOfCombinedMeshInHierarchy = 0;
        private bool canStartMerge = false;

        public static void OpenWindow()
        {
            //Method to open the Window
            var window = GetWindow<MeshCombinerTool>("Combiner Tool");
            window.minSize = new Vector2(600, 650);
            window.maxSize = new Vector2(600, 650);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();
        }

        //UI Code
        #region INTERFACE_CODE
        void OnEnable()
        {
            //On enable this window, on re-start this window after compilation
            isWindowOnFocus = true;

            //Load the preferences
            LoadThePreferences(this);

            //Register the OnSceneGUI
#if !UNITY_2019_1_OR_NEWER
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
#endif
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif
        }

        void OnDisable()
        {
            //On disable this window, after compilation, disables the window and enable again
            isWindowOnFocus = false;

            //Save the preferences
            SaveThePreferences(this);

            //Unregister the OnSceneGUI
#if !UNITY_2019_1_OR_NEWER
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
#endif
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += this.OnSceneGUI;
#endif
        }

        void OnDestroy()
        {
            //On close this window
            isWindowOnFocus = false;

            //Save the preferences
            SaveThePreferences(this);
        }

        void OnFocus()
        {
            //On focus this window
            isWindowOnFocus = true;
        }

        void OnLostFocus()
        {
            //On lose focus in window
            isWindowOnFocus = false;
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Try to load needed assets
            Texture iconOfUi = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Easy Mesh Combiner/Editor/Images/Icon.png", typeof(Texture));
            Texture iconDoneOfUi = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Easy Mesh Combiner/Editor/Images/IconDone.png", typeof(Texture));
            Texture arrowIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Easy Mesh Combiner/Editor/Images/Arrow.png", typeof(Texture));
            Texture arrowDoneIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Easy Mesh Combiner/Editor/Images/ArrowDone.png", typeof(Texture));
            //If fails on load needed assets, locks ui
            if(iconOfUi == null || arrowIcon == null || arrowDoneIcon == null || iconDoneOfUi == null)
            {
                EditorGUILayout.HelpBox("Unable to load required files. Please reinstall Easy Mesh Combiner to correct this problem.", MessageType.Error);
                return;
            }

            //Validate the selection in each update of this UI
            ValidateSelection();

            //Topbar
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(8);
            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUIStyle estiloIcone = new GUIStyle();
            estiloIcone.border = new RectOffset(0, 0, 0, 0);
            estiloIcone.margin = new RectOffset(4, 0, 4, 0);
            if (mergeIsDone == false)
            {
                GUILayout.Box(iconOfUi, estiloIcone, GUILayout.Width(48), GUILayout.Height(44));
            }
            if (mergeIsDone == true)
            {
                GUILayout.Box(iconDoneOfUi, estiloIcone, GUILayout.Width(48), GUILayout.Height(44));
            }
            GUILayout.Space(6);
            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUILayout.Space(-110);
            GUILayout.BeginVertical();
            GUILayout.Space(14);
            GUIStyle titulo = new GUIStyle();
            titulo.fontSize = 25;
            titulo.normal.textColor = Color.black;
            titulo.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Easy Mesh Combiner", titulo);
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if(mergeIsDone == false)
            {
                GUIStyle subTitulo = new GUIStyle();
                subTitulo.fontSize = 11;
                subTitulo.alignment = TextAnchor.MiddleLeft;
                if (gameObjectsSelected.Length == 0)
                {
                    EditorGUILayout.LabelField("No GameObject has been selected.", subTitulo);
                }
                if (gameObjectsSelected.Length > 0)
                {
                    EditorGUILayout.LabelField(gameObjectsSelected.Length.ToString() + " GameObject's selected. " + validsGameObjects.Count.ToString() + " valid meshes found. " + invalidsGameObjectsCount.ToString() + " meshes ignored.", subTitulo);
                }
            }
            if(mergeIsDone == true)
            {
                GUIStyle subTitulo = new GUIStyle();
                subTitulo.fontSize = 11;
                subTitulo.fontStyle = FontStyle.Bold;
                subTitulo.alignment = TextAnchor.MiddleLeft;
                subTitulo.normal.textColor = new Color(7f / 100.0f, 69f / 100.0f, 7f / 100.0f, 1);
                EditorGUILayout.LabelField("The merge has been completed.", subTitulo);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUIStyle tituloBox = new GUIStyle();
            tituloBox.fontStyle = FontStyle.Bold;
            tituloBox.alignment = TextAnchor.MiddleCenter;

            //Preferences
            GUILayout.BeginVertical("box");
            scrollPosPreferences = EditorGUILayout.BeginScrollView(scrollPosPreferences, GUILayout.Width(284), GUILayout.Height(361));
            EditorGUILayout.LabelField("Preferences of Merge", tituloBox);

            GUILayout.Space(20);
            int spaceToRememberMessage = 168;

            meshCombinerPreferences.afterMerge = (MeshCombinerPreferences.AfterMerge)EditorGUILayout.EnumPopup(new GUIContent("After Combine",
                                "What do you do after you complete the merge?\n\nDisable Original Meshes - The original meshes will be deactivated, so all the colliders and other components of the scenario will be kept intact, but the meshes will still be combined!\n\nDeactive Original GameObjects - All original GameObjects will be disabled. When you do not need to keep colliders and other active components in the scene, this is a good option!\n\nRelax, however, it is possible to undo the merge later and re-activate everything again!"),
                                meshCombinerPreferences.afterMerge);

            meshCombinerPreferences.combineChildrens = (bool)EditorGUILayout.Toggle(new GUIContent("Combine Children",
                        "If you want to combine children's of the selected GameObjects, enable this option!"),
                        meshCombinerPreferences.combineChildrens);

            if (meshCombinerPreferences.combineChildrens == true)
            {
                EditorGUI.indentLevel += 1;
                meshCombinerPreferences.combineInactives = (bool)EditorGUILayout.Toggle(new GUIContent("Combine Inactives",
                        "If you want to combine the GameObjects children that are disabled, just enable this option."),
                        meshCombinerPreferences.combineInactives);
                spaceToRememberMessage -= 18;
                EditorGUI.indentLevel -= 1;
            }

            meshCombinerPreferences.lightmapSupport = (bool)EditorGUILayout.Toggle(new GUIContent("Lightmap Support",
                        "If you will use the lightmaps, enable this option so that the merged meshes can support it.\n\nNote that by enabling this option, the merged mesh will have more vertices than it should have, and if the vertex count exceeds 64k, support for lightmaps in that mesh will be canceled.\n\n** Keep in mind that enabling this option can greatly increase mescaling's processing time! **"),
                        meshCombinerPreferences.lightmapSupport);

            meshCombinerPreferences.saveMeshInAssets = (bool)EditorGUILayout.Toggle(new GUIContent("Save Mesh In Assets",
                        "After matching the meshes, the resulting mesh will be saved in your project files. That way, you will not lose the combined mesh and you can still build your game with the combined scenario!"),
                        meshCombinerPreferences.saveMeshInAssets);

            meshCombinerPreferences.savePrefabOfThis = (bool)EditorGUILayout.Toggle(new GUIContent("Save Prefab Of This",
                        "After merge, Easy Mesh Combiner will save the prefab of this merge to your project files."),
                        meshCombinerPreferences.savePrefabOfThis);

            if (meshCombinerPreferences.savePrefabOfThis == true)
            {
                meshCombinerPreferences.saveMeshInAssets = true;
                EditorGUI.indentLevel += 1;
                meshCombinerPreferences.prefabName = EditorGUILayout.TextField(new GUIContent("Prefab Name",
                                "The name that will be given to the prefab generated after the merge."),
                                meshCombinerPreferences.prefabName);
                if (meshCombinerPreferences.prefabName == "")
                {
                    DateTime now = DateTime.Now;
                    meshCombinerPreferences.prefabName = "prefab_of_merge_" + now.Ticks;
                }
                spaceToRememberMessage -= 18;
                EditorGUI.indentLevel -= 1;
            }

            meshCombinerPreferences.nameOfThisMerge = (string)EditorGUILayout.TextField(new GUIContent("Name Of This Merge",
                        "The name that will be given to GameObject resulting from this merge."),
                        meshCombinerPreferences.nameOfThisMerge);

            GUILayout.Space(spaceToRememberMessage);

            EditorGUILayout.HelpBox("Remember to read the Easy Mesh Combiner documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            //Logs of Merge
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Logs of Merge (" + logsOfMerge.Count.ToString() + ")", tituloBox);
            scrollPosLogs = EditorGUILayout.BeginScrollView(scrollPosLogs, GUILayout.Width(293), GUILayout.Height(325));
            for (int i = 0; i < logsOfMerge.Count; i++)
            {
                EditorGUILayout.HelpBox(logsOfMerge[i].message, logsOfMerge[i].logType);
            }
            EditorGUILayout.EndScrollView();
            //Set the scroll of logs to end, if has new logs
            if (logsOfMerge.Count != lastQuantityOfLogs)
            {
                scrollPosLogs.y += 99999;
                lastQuantityOfLogs = logsOfMerge.Count;
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(60);
            meshCombinerPreferences.representLogsInScene = (bool)EditorGUILayout.Toggle(new GUIContent("Represent logs in scene",
                        "Check this option to have Easy Mesh Combiner represent in your scene the valid and invalid meshes found in your selection."),
                        meshCombinerPreferences.representLogsInScene);
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //Stats bar
            GUILayout.BeginHorizontal("box");
            scrollPosStats = EditorGUILayout.BeginScrollView(scrollPosStats, GUILayout.Width(587), GUILayout.Height(127));
            EditorGUILayout.LabelField("Current Statistics And After Do This Merge", tituloBox);
            if (validsGameObjects.Count == 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(40);
                GUIStyle noGameObjectsStats = new GUIStyle();
                noGameObjectsStats.alignment = TextAnchor.MiddleCenter;
                EditorGUILayout.LabelField("No valid meshes selected.", noGameObjectsStats);
                GUILayout.EndVertical();
            }
            if (validsGameObjects.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Space(8);
                GUIStyle gameObjectsStatsBefore = new GUIStyle();
                gameObjectsStatsBefore.alignment = TextAnchor.MiddleLeft;
                EditorGUILayout.LabelField("Vertex Count: " + statisticsBeforeMerge.totalVertex, gameObjectsStatsBefore);
                EditorGUILayout.LabelField("Meshes Count: " + statisticsBeforeMerge.meshesAndSubmeshesCount, gameObjectsStatsBefore);
                EditorGUILayout.LabelField("Materials Count: " + statisticsBeforeMerge.materialsCount, gameObjectsStatsBefore);
                EditorGUILayout.LabelField("Draw Calls ± " + statisticsBeforeMerge.drawCallsAproximate, gameObjectsStatsBefore);
                EditorGUILayout.LabelField("Optimization Rate: " + statisticsBeforeMerge.optimizationRate + "%", gameObjectsStatsBefore);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Space(36);
                if (mergeIsDone == false)
                {
                    GUILayout.Box(arrowIcon, estiloIcone, GUILayout.Width(40), GUILayout.Height(44));
                }
                if (mergeIsDone == true)
                {
                    GUILayout.Box(arrowDoneIcon, estiloIcone, GUILayout.Width(40), GUILayout.Height(44));
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Space(8);
                GUIStyle gameObjectsStatsAfter = new GUIStyle();
                gameObjectsStatsAfter.alignment = TextAnchor.MiddleRight;
                EditorGUILayout.LabelField("Vertex Count: " + statisticsAfterMerge.totalVertex, gameObjectsStatsAfter);
                EditorGUILayout.LabelField("Meshes Count: " + statisticsAfterMerge.meshesAndSubmeshesCount, gameObjectsStatsAfter);
                EditorGUILayout.LabelField("Materials Count: " + statisticsAfterMerge.materialsCount, gameObjectsStatsAfter);
                EditorGUILayout.LabelField("Draw Calls ± " + statisticsAfterMerge.drawCallsAproximate, gameObjectsStatsAfter);
                EditorGUILayout.LabelField("Optimization Rate: " + statisticsAfterMerge.optimizationRate.ToString("F1") + "%", gameObjectsStatsAfter);
                GUILayout.EndVertical(); 
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            //Bottom bar
            GUILayout.BeginHorizontal("box");
            if (canStartMerge == false)
            {
                GUILayout.Space(157);
                GUILayout.BeginVertical();
                GUILayout.Space(12);
                EditorGUILayout.HelpBox("Cannot merge GameObjects and meshes. Check the Logs above to understand why.", MessageType.Warning);
                GUILayout.Space(11);
                GUILayout.EndVertical();
                GUILayout.Space(153);
            }
            if (canStartMerge == true)
            {
                GUILayout.Space(200);
                GUILayout.BeginVertical();
                GUILayout.Space(7);
                if(mergeIsDone == false)
                {
                    if (GUILayout.Button("Combine Meshes!", GUILayout.Height(49)))
                    {
                        //Do the scene dirty
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                        //Save the preferences
                        SaveThePreferences(this);

                        //Start the merge
                        CombineMeshes();
                    }
                }
                if (mergeIsDone == true)
                {
                    if (GUILayout.Button("Ok, Close This!", GUILayout.Height(49)))
                    {
                        //Save the preferences
                        SaveThePreferences(this);

                        //Close the window
                        this.Close();
                    }
                }
                GUILayout.Space(6);
                GUILayout.EndVertical();
                GUILayout.Space(200);
            }
            GUILayout.EndHorizontal();

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            //If a merge already is done, not runs the highlight
            if(mergeIsDone == true)
            {
                return;
            }

            //Show the selected GameObject in scene GUI, if is enabled. Only works if merge is not ended.
            if(meshCombinerPreferences.representLogsInScene == true)
            {
                foreach (SelectedItem selectedItem in selectedGameObjectsToShow)
                {
                    //If the selected item has deleted, continues to next
                    if(selectedItem.gameObject == null)
                    {
                        continue;
                    }

                    if (selectedItem.isValidGameObject == true)
                    {
                        Handles.color = Color.blue;
                        Bounds bounds = selectedItem.meshRenderer.bounds;
                        float multiplier = 1.0f;
                        Handles.DrawWireCube(selectedItem.gameObject.transform.position, new Vector3(bounds.size.x * multiplier, bounds.size.y * multiplier, bounds.size.z * multiplier));
                    }
                    if (selectedItem.isValidGameObject == false)
                    {
                        Handles.color = Color.red;
                        Handles.SphereHandleCap(0, selectedItem.gameObject.transform.position, Quaternion.identity, 0.5f, EventType.Repaint);
                    }
                }
            }
        }

        void OnInspectorUpdate()
        {
            //On inspector update, on lost focus in this Window
            if (isWindowOnFocus == false)
            {
                //Update this window
                Repaint();
                //Update the scene GUI
                if(SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
            }

            //Try to load the preferences on inspector update (if this window is in focus or not, try to load here, because this method runs after OpenWindow() method)
            if (preferencesLoadedOnInspectorUpdate == false)
            {
                if(meshCombinerPreferences.windowPosition.x != 0 && meshCombinerPreferences.windowPosition.y != 0)
                {
                    LoadThePreferences(this);
                }
                preferencesLoadedOnInspectorUpdate = true;
            }
        }
        #endregion

        void ValidateSelection()
        {
            //If a merge already is done, not run the validation, and mantains the last validation
            if(mergeIsDone == true)
                return;

            //Get the selected objects
            gameObjectsSelected = Selection.gameObjects;

            //Calculate the index of gameobject result of merge, in hierarchy of scene. put it after last gameobject selected on hierarchy
            if (gameObjectsSelected.Length > 0)
            {
                int majorSiblingIndex = 0;
                foreach (GameObject obj in gameObjectsSelected)
                {
                    if(obj.transform.GetSiblingIndex() > majorSiblingIndex)
                        majorSiblingIndex = obj.transform.root.GetSiblingIndex();
                }
                targetIndexOfCombinedMeshInHierarchy = majorSiblingIndex + 1;
            }

            //Clear the valid gameobjects to re-validate, and reset count of vertices
            validsGameObjects.Clear();
            invalidsGameObjectsCount = 0;
            canStartMerge = true;
            int vertexCountInValidGos = 0;

            //Reset statstics
            if (statisticsBeforeMerge == null)
                statisticsBeforeMerge = new StatisticsOfMerge();
            if (statisticsAfterMerge == null)
                statisticsAfterMerge = new StatisticsOfMerge();
            statisticsBeforeMerge.totalVertex = 0;
            statisticsBeforeMerge.materialsCount = 0;
            statisticsBeforeMerge.meshesAndSubmeshesCount = 0;
            statisticsBeforeMerge.drawCallsAproximate = 0;
            statisticsBeforeMerge.optimizationRate = 0;
            statisticsAfterMerge.totalVertex = 0;
            statisticsAfterMerge.materialsCount = 0;
            statisticsAfterMerge.meshesAndSubmeshesCount = 0;
            statisticsAfterMerge.drawCallsAproximate = 0;
            statisticsAfterMerge.optimizationRate = 0;

            //Clear the list of logs of merge, for regenarate
            logsOfMerge.Clear();

            //Clear the selected gameobjects of scene GUI
            selectedGameObjectsToShow.Clear();

            //Get all found gameobjects in this selection, with parameters
            List<Transform> foundGameObjects = new List<Transform>();
            for (int i = 0; i < gameObjectsSelected.Length; i++)
            {
                if (meshCombinerPreferences.combineChildrens == true)
                {
                    Transform[] childrenGameObjectsInThis = gameObjectsSelected[i].GetComponentsInChildren<Transform>(true);
                    foreach (Transform trs in childrenGameObjectsInThis)
                        foundGameObjects.Add(trs);
                }
                if (meshCombinerPreferences.combineChildrens == false)
                    foundGameObjects.Add(gameObjectsSelected[i].GetComponent<Transform>());
            }

            //Verify if has found gameObjects
            if (foundGameObjects.Count == 0)
            {
                logsOfMerge.Add(new LogOfMerge(MessageType.Info, "No GameObject has been selected. Select at least 1 GameObject so that Easy Mesh Combiner can work."));
                canStartMerge = false;
                return;
            }

            //Validate each found gameobject and split gameobjects that contains mesh filter or/and mesh renderer, and add to list of valid gameObjects
            List<GameObjectWithMesh> gameObjectsWithMesh = new List<GameObjectWithMesh>();
            for (int i = 0; i < foundGameObjects.Count; i++)
            {
                MeshFilter mf = foundGameObjects[i].GetComponent<MeshFilter>();
                MeshRenderer mr = foundGameObjects[i].GetComponent<MeshRenderer>();
                if (mf != null || mr != null)
                {
                    //If combine inactives is disabled, and mesh filter component/gameobject is disabled in this object, skips this
                    if (meshCombinerPreferences.combineInactives == false && mr.enabled == false)
                        continue;
                    if (meshCombinerPreferences.combineInactives == false && foundGameObjects[i].gameObject.activeSelf == false)
                        continue;

                    gameObjectsWithMesh.Add(new GameObjectWithMesh(foundGameObjects[i].gameObject, mf, mr));
                }  
            }

            //Verify if each gameObject with mesh, is valid and have correct components settings
            for (int i = 0; i < gameObjectsWithMesh.Count; i++)
            {
                bool canAddToValidGameObjects = true;

                //Verify if MeshFilter is null
                if (gameObjectsWithMesh[i].meshFilter == null)
                {
                    logsOfMerge.Add(new LogOfMerge(MessageType.Error, "GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" does not have the Mesh Filter component, so it is not a valid mesh and will be ignored in the merge process."));
                    canAddToValidGameObjects = false;
                }
                //Verify if MeshRenderer is null
                if (gameObjectsWithMesh[i].meshRenderer == null)
                {
                    logsOfMerge.Add(new LogOfMerge(MessageType.Error, "GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" does not have the Mesh Renderer component, so it is not a valid mesh and will be ignored in the merge process."));
                    canAddToValidGameObjects = false;
                }
                //Verify if SharedMesh is null
                if (gameObjectsWithMesh[i].meshFilter != null && gameObjectsWithMesh[i].meshFilter.sharedMesh == null)
                {
                    logsOfMerge.Add(new LogOfMerge(MessageType.Error, "GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" does not have a Mesh in Mesh Filter component, so it is not a valid mesh and will be ignored in the merge process."));
                    canAddToValidGameObjects = false;
                }
                //Verify if count of materials is different of count of submeshes
                if (gameObjectsWithMesh[i].meshFilter != null && gameObjectsWithMesh[i].meshRenderer != null && gameObjectsWithMesh[i].meshFilter.sharedMesh != null)
                {
                    if (gameObjectsWithMesh[i].meshFilter.sharedMesh.subMeshCount != gameObjectsWithMesh[i].meshRenderer.sharedMaterials.Length)
                    {
                        logsOfMerge.Add(new LogOfMerge(MessageType.Error, "The Mesh Renderer component found in GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" has more or less material needed. The mesh that is in this GameObject has " + gameObjectsWithMesh[i].meshFilter.sharedMesh.subMeshCount.ToString() + " submeshes, but has a number of " + gameObjectsWithMesh[i].meshRenderer.sharedMaterials.Length.ToString() + " materials. This mesh will be ignored during the merge process."));
                        canAddToValidGameObjects = false;
                    }
                }
                //Verify if has null materials in MeshRenderer
                if (gameObjectsWithMesh[i].meshRenderer != null)
                {
                    for (int x = 0; x < gameObjectsWithMesh[i].meshRenderer.sharedMaterials.Length; x++)
                    {
                        if (gameObjectsWithMesh[i].meshRenderer.sharedMaterials[x] == null)
                        {
                            logsOfMerge.Add(new LogOfMerge(MessageType.Error, "Material " + x.ToString() + " in Mesh Renderer present in component \"" + gameObjectsWithMesh[i].gameObject.name + "\" is null. For the merge process to work well, all materials must be completed. This GameObject will be ignored in the merge process."));
                            canAddToValidGameObjects = false;
                        }
                    }
                    //If this GameObjects contains more than 2 materials, add to the list of warning of many materials
                    if (gameObjectsWithMesh[i].meshFilter != null && gameObjectsWithMesh[i].meshFilter.sharedMesh != null && gameObjectsWithMesh[i].meshFilter.sharedMesh.vertexCount > 1500 && gameObjectsWithMesh[i].meshRenderer.sharedMaterials.Length > 2 && meshCombinerPreferences.lightmapSupport == true)
                    {
                        logsOfMerge.Add(new LogOfMerge(MessageType.Warning, "The mesh in GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" contains many vertices and a large amount of materials. Due to a Unity limitation, you may experience a longer time to combine meshes using lightmap support, or you may experience errors during the merge process. If this happens, try reducing the amount of sub-malhas present in this mesh. If no problem occurs, do not worry, everything went as expected."));
                    }
                }
                //Verify if this gameobject is already merged
                if (gameObjectsWithMesh[i].gameObject.GetComponent<CombinedMeshesManager>() != null)
                {
                    logsOfMerge.Add(new LogOfMerge(MessageType.Error, "GameObject \"" + gameObjectsWithMesh[i].gameObject.name + "\" is the result of a previous merge, so it will be ignored by this merge."));
                    canAddToValidGameObjects = false;
                }

                //If can add to valid GameObjects, add this gameobject
                if(canAddToValidGameObjects == true)
                {
                    validsGameObjects.Add(gameObjectsWithMesh[i]);
                    vertexCountInValidGos += gameObjectsWithMesh[i].meshFilter.sharedMesh.vertexCount;

                    //Incremente statistics of before
                    statisticsBeforeMerge.totalVertex += gameObjectsWithMesh[i].meshFilter.sharedMesh.vertexCount;
                    statisticsBeforeMerge.meshesAndSubmeshesCount += gameObjectsWithMesh[i].meshFilter.sharedMesh.subMeshCount;
                    statisticsBeforeMerge.drawCallsAproximate += gameObjectsWithMesh[i].meshFilter.sharedMesh.subMeshCount;
                    statisticsBeforeMerge.optimizationRate = 0.0f;

                    //Incremente statistics of after
                    statisticsAfterMerge.totalVertex += gameObjectsWithMesh[i].meshFilter.sharedMesh.vertexCount;

                    //Add this gameobject of list in selected gameobjects
                    selectedGameObjectsToShow.Add(new SelectedItem(gameObjectsWithMesh[i].gameObject, gameObjectsWithMesh[i].meshFilter, gameObjectsWithMesh[i].meshRenderer, true));
                }
                //If cannot add to valid gameobjects, add to list of selected gameobjects to show in scene gui
                if(canAddToValidGameObjects == false)
                {
                    //Add this gameobject of list in selected gameobjects
                    selectedGameObjectsToShow.Add(new SelectedItem(gameObjectsWithMesh[i].gameObject, gameObjectsWithMesh[i].meshFilter, gameObjectsWithMesh[i].meshRenderer, false));
                    invalidsGameObjectsCount += 1;
                }
            }
            //Scan the valid gameobjects for found unique materials count statistics
            Dictionary<Material, bool> uniqueMaterials = new Dictionary<Material, bool>();
            foreach (GameObjectWithMesh gowm in validsGameObjects)
            {
                for (int i = 0; i < gowm.meshRenderer.sharedMaterials.Length; i++)
                {
                    if (uniqueMaterials.ContainsKey(gowm.meshRenderer.sharedMaterials[i]) == false)
                    {
                        uniqueMaterials.Add(gowm.meshRenderer.sharedMaterials[i], true);
                    }
                }
            }
            statisticsBeforeMerge.materialsCount = uniqueMaterials.Keys.Count;
            statisticsAfterMerge.materialsCount = uniqueMaterials.Keys.Count;
            statisticsAfterMerge.meshesAndSubmeshesCount = uniqueMaterials.Keys.Count;
            statisticsAfterMerge.drawCallsAproximate = uniqueMaterials.Keys.Count;
            statisticsAfterMerge.optimizationRate = (1 - ((float)uniqueMaterials.Keys.Count / (float)statisticsBeforeMerge.meshesAndSubmeshesCount)) * (float)100;

            //Verify if can start merge
            if (validsGameObjects.Count == 0)
            {
                logsOfMerge.Add(new LogOfMerge(MessageType.Warning, "Cannot start a merge as there are no valid meshes in the selected GameObjects. Please select GameObjects that contain valid meshes so that the merge can be done. For the merge process to be done, there must be at least 1 valid and active mesh and found in your selection."));
                canStartMerge = false;
            }

            //Update the scene GUI
            if(SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.Repaint();
        }

        static void LoadThePreferences(MeshCombinerTool instance)
        {
            //Create the default directory, if not exists
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Preferences"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Preferences");

            //Try to load the preferences file
            meshCombinerPreferences = (MeshCombinerPreferences)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Preferences/EasyMeshCombiner.asset", typeof(MeshCombinerPreferences));
            //Validate the preference file. if this preference file is of another project, delete then
            if (meshCombinerPreferences != null)
            {
                if (meshCombinerPreferences.projectName != Application.productName)
                {
                    AssetDatabase.DeleteAsset("Assets/MT Assets/_AssetsData/Preferences/EasyMeshCombiner.asset");
                    meshCombinerPreferences = null;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                if (meshCombinerPreferences != null && meshCombinerPreferences.projectName == Application.productName)
                {
                    //Set the position of Window 
                    instance.position = meshCombinerPreferences.windowPosition;
                }
            }
            //If null, create and save a preferences file
            if (meshCombinerPreferences == null)
            {
                meshCombinerPreferences = ScriptableObject.CreateInstance<MeshCombinerPreferences>();
                meshCombinerPreferences.projectName = Application.productName;
                AssetDatabase.CreateAsset(meshCombinerPreferences, "Assets/MT Assets/_AssetsData/Preferences/EasyMeshCombiner.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SaveThePreferences(MeshCombinerTool instance)
        {
            //Save the preferences in Prefs.asset
            meshCombinerPreferences.projectName = Application.productName;
            meshCombinerPreferences.windowPosition = new Rect(instance.position.x, instance.position.y, instance.position.width, instance.position.height);
            EditorUtility.SetDirty(meshCombinerPreferences);
            AssetDatabase.SaveAssets();
        }

        //Merge code

        public class SubMeshToCombine
        {
            //Class that stores a mesh filter/renderer and respective submesh index, to combine
            public Transform transform;
            public MeshFilter meshFilter;
            public MeshRenderer meshRenderer;
            public int subMeshIndex;

            public SubMeshToCombine(Transform transform, MeshFilter meshFilter, MeshRenderer meshRenderer, int subMeshIndex)
            {
                this.transform = transform;
                this.meshFilter = meshFilter;
                this.meshRenderer = meshRenderer;
                this.subMeshIndex = subMeshIndex;
            }
        }

        void CombineMeshes()
        {
            //Show progress dialog
            EditorUtility.DisplayProgressBar("Merging", "A moment...", 1);

            //Separate each submesh according to your material
            Dictionary<Material, List<SubMeshToCombine>> subMeshesPerMaterial = new Dictionary<Material, List<SubMeshToCombine>>();
            for (int i = 0; i < validsGameObjects.Count; i++)
            {
                GameObjectWithMesh thisGoWithMesh = validsGameObjects[i];

                for (int x = 0; x < thisGoWithMesh.meshFilter.sharedMesh.subMeshCount; x++)
                {
                    Material currentMaterial = thisGoWithMesh.meshRenderer.sharedMaterials[x];
                    if (subMeshesPerMaterial.ContainsKey(currentMaterial) == true)
                    {
                        subMeshesPerMaterial[currentMaterial].Add(new SubMeshToCombine(thisGoWithMesh.gameObject.transform, thisGoWithMesh.meshFilter, thisGoWithMesh.meshRenderer, x));
                    }
                    if (subMeshesPerMaterial.ContainsKey(currentMaterial) == false)
                    {
                        subMeshesPerMaterial.Add(currentMaterial, new List<SubMeshToCombine>() { new SubMeshToCombine(thisGoWithMesh.gameObject.transform, thisGoWithMesh.meshFilter, thisGoWithMesh.meshRenderer, x) });
                    }
                }
            }

            //Create the holder GameObject
            GameObject holderGameObject = new GameObject(meshCombinerPreferences.nameOfThisMerge);
            CombinedMeshesManager holderManager = holderGameObject.AddComponent<CombinedMeshesManager>();
            MeshFilter holderMeshFilter = holderGameObject.AddComponent<MeshFilter>();
            MeshRenderer holderMeshRenderer = holderGameObject.AddComponent<MeshRenderer>();
            if (meshCombinerPreferences.lightmapSupport == false)
            {
                GameObjectUtility.SetStaticEditorFlags(holderGameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
            }
            if (meshCombinerPreferences.lightmapSupport == true)
            {
                GameObjectUtility.SetStaticEditorFlags(holderGameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI);
            }
            holderGameObject.transform.SetSiblingIndex(targetIndexOfCombinedMeshInHierarchy);

            //Combine the submeshes into one submesh according the material
            List<Mesh> combinedSubmehesPerMaterial = new List<Mesh>();
            foreach (var key in subMeshesPerMaterial)
            {
                //Get the submeshes to merge, of current material
                List<SubMeshToCombine> subMeshesOfCurrentMaterial = key.Value;

                //Combine instances of submeshes from this material
                List<CombineInstance> combineInstancesOfCurrentMaterial = new List<CombineInstance>();

                //Count of vertices for all submeshes of this material
                int totalVerticesCount = 0;

                //Process each submesh
                for (int i = 0; i < subMeshesOfCurrentMaterial.Count; i++)
                {
                    CombineInstance combineInstance = new CombineInstance();
                    combineInstance.mesh = subMeshesOfCurrentMaterial[i].meshFilter.sharedMesh;
                    combineInstance.subMeshIndex = subMeshesOfCurrentMaterial[i].subMeshIndex;
                    combineInstance.transform = subMeshesOfCurrentMaterial[i].transform.localToWorldMatrix;
                    combineInstancesOfCurrentMaterial.Add(combineInstance);
                    totalVerticesCount += combineInstance.mesh.vertexCount;
                }

                //Create the submesh with all submeshes with current material, and set limitation of vertices
                Mesh mesh = new Mesh();
#if UNITY_2017_4 || UNITY_2018_1_OR_NEWER
                if (totalVerticesCount <= MAX_VERTICES_FOR_16BITS_MESH)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
                if (totalVerticesCount > MAX_VERTICES_FOR_16BITS_MESH)
                    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
                mesh.CombineMeshes(combineInstancesOfCurrentMaterial.ToArray(), true, true, meshCombinerPreferences.lightmapSupport);

                //Add to list of combined submeshes per material
                combinedSubmehesPerMaterial.Add(mesh);
            }

            //Process each combined submeshes per material, creating final combine instances
            List<CombineInstance> finalCombineInstances = new List<CombineInstance>();
            int totalFinalVerticesCount = 0;
            foreach (Mesh mesh in combinedSubmehesPerMaterial)
            {
                CombineInstance combineInstanceOfThisSubMesh = new CombineInstance();
                combineInstanceOfThisSubMesh.mesh = mesh;
                combineInstanceOfThisSubMesh.subMeshIndex = 0;
                combineInstanceOfThisSubMesh.transform = Matrix4x4.identity;
                finalCombineInstances.Add(combineInstanceOfThisSubMesh);
                totalFinalVerticesCount += combineInstanceOfThisSubMesh.mesh.vertexCount;
            }

            //Create the final mesh that contains all submeshes divided per material
            Mesh finalMesh = new Mesh();
#if UNITY_2017_4 || UNITY_2018_1_OR_NEWER
            if (totalFinalVerticesCount <= MAX_VERTICES_FOR_16BITS_MESH)
                finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            if (totalFinalVerticesCount > MAX_VERTICES_FOR_16BITS_MESH)
                finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#endif
            finalMesh.CombineMeshes(finalCombineInstances.ToArray(), false);
            finalMesh.RecalculateBounds();
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateTangents();
            if (meshCombinerPreferences.lightmapSupport == true)
            {
                Unwrapping.GenerateSecondaryUVSet(finalMesh);
            }

            //Polulate the holder GameObject with the data of combined mesh
            holderMeshFilter.sharedMesh = finalMesh;
            List<Material> materialsForSubMeshes = new List<Material>();
            foreach (var key in subMeshesPerMaterial)
            {
                materialsForSubMeshes.Add(key.Key);
            }
            holderMeshRenderer.sharedMaterials = materialsForSubMeshes.ToArray();

            //Save the mesh of merge in assets, if desired
            if(meshCombinerPreferences.saveMeshInAssets == true)
            {
                holderManager.pathsOfAssetToDelete = SaveMergeDataInAssets(meshCombinerPreferences.nameOfThisMerge, holderMeshFilter);
            }

            //Save a prefab, if is desired
            if(meshCombinerPreferences.savePrefabOfThis == true)
            {
                holderManager.thisIsPrefab = true;
                SavePrefabInAssets(meshCombinerPreferences.prefabName, holderGameObject);
            }

            //Deactive original GameObjects if is desired
            if (meshCombinerPreferences.afterMerge == MeshCombinerPreferences.AfterMerge.DeactiveOriginalGameObjects)
            {
                holderManager.undoMethod = CombinedMeshesManager.UndoMethod.ReactiveOriginalGameObjects;
                foreach (GameObjectWithMesh obj in validsGameObjects)
                {
                    holderManager.originalGosToRestore.Add(new CombinedMeshesManager.OriginalGameObjectWithMesh(obj.gameObject, obj.gameObject.activeSelf, obj.meshRenderer, obj.meshRenderer.enabled));
                    obj.gameObject.SetActive(false);
                }
            }
            //Disable original mesh filters and renderers if is desired
            if (meshCombinerPreferences.afterMerge == MeshCombinerPreferences.AfterMerge.DisableOriginalMeshes)
            {
                holderManager.undoMethod = CombinedMeshesManager.UndoMethod.EnableOriginalMeshes;
                foreach (GameObjectWithMesh obj in validsGameObjects)
                {
                    holderManager.originalGosToRestore.Add(new CombinedMeshesManager.OriginalGameObjectWithMesh(obj.gameObject, obj.gameObject.activeSelf, obj.meshRenderer, obj.meshRenderer.enabled));
                    obj.meshRenderer.enabled = false;
                }
            }

            //Set scene dirty and refresh asset data
            AssetDatabase.Refresh();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            //Hide progress dialog
            EditorUtility.ClearProgressBar();

            //Select and ping the gameobject of merge
            Selection.activeGameObject = holderGameObject;
            EditorGUIUtility.PingObject(holderGameObject);

            //Add the log
            logsOfMerge.Add(new LogOfMerge(MessageType.Info, "The merge has been successfully completed! See merge statistics in the box below."));

            //Report that the merge is ended
            mergeIsDone = true;
        }

        string SaveMergeDataInAssets(string name, MeshFilter meshFilter)
        {
            //Save the merge data in assets
            DateTime dateTime = new DateTime();
            dateTime = DateTime.Now;

            //Create the directory in project
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Meshes"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Meshes");

            //Save the mesh data
            string pathToMeshFile = "Assets/MT Assets/_AssetsData/Meshes/" + name + " (" + dateTime.Year + dateTime.Month + dateTime.Day + dateTime.Hour + dateTime.Minute + dateTime.Second + dateTime.Millisecond.ToString() + ").asset";
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, pathToMeshFile);
            return pathToMeshFile;
        }

        void SavePrefabInAssets(string name, GameObject targetGo)
        {
            //Save the GameObject result of merge, in assets
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Prefabs"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Prefabs");

            if (AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Prefabs/" + name + ".prefab", typeof(GameObject)) != null)
                Debug.LogWarning("Prefab \"" + name + "\" already exists in your project files. Therefore, a new file was not created.\n\n");
            if (AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Prefabs/" + name + ".prefab", typeof(GameObject)) == null)
            {
#if !UNITY_2018_3_OR_NEWER
                UnityEngine.Object prefab = PrefabUtility.CreatePrefab("Assets/MT Assets/_AssetsData/Prefabs/" + name + ".prefab", targetGo);
                PrefabUtility.ReplacePrefab(targetGo, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
#if UNITY_2018_3_OR_NEWER
                PrefabUtility.SaveAsPrefabAssetAndConnect(targetGo, "Assets/MT Assets/_AssetsData/Prefabs/" + name + ".prefab", InteractionMode.UserAction);
#endif
                Debug.Log("The prefab \"" + name + "\" was created in your project files! The path to the prefabs that the Easy Mesh Combiner creates is the \"Assets/MT Assets/_AssetsData/Prefabs\"\n\n");
            }
        }
    }
}