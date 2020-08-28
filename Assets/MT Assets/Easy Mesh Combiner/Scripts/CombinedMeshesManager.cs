#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
using System.IO;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.EasyMeshCombiner
{
    /*
      This class is responsible for the functioning of the "Combined Meshes Manager" component, and all its functions.
    */
    /*
     * The Easy Mesh Combiner was developed by Marcos Tomaz in 2019.
     * Need help? Contact me (mtassets@windsoft.xyz)
     */

    [AddComponentMenu("")] //Hide this script in component menu.
    public class CombinedMeshesManager : MonoBehaviour
    {
#if UNITY_EDITOR
        //Public variables of Interface
        private bool gizmosOfThisComponentIsDisabled = false;

        //Variables of script
        [System.Serializable]
        public class OriginalGameObjectWithMesh
        {
            //Class that stores a original GameObject With Mesh data, to restore on undo merge.

            public GameObject gameObject;
            public bool originalGoState;
            public MeshRenderer meshRenderer;
            public bool originalMrState;

            public OriginalGameObjectWithMesh(GameObject gameObject, bool originalGoState, MeshRenderer meshRenderer, bool originalMrState)
            {
                this.gameObject = gameObject;
                this.originalGoState = originalGoState;
                this.meshRenderer = meshRenderer;
                this.originalMrState = originalMrState;
            }
        }
        public enum UndoMethod
        {
            EnableOriginalMeshes,
            ReactiveOriginalGameObjects
        }
        [HideInInspector]
        public UndoMethod undoMethod;
        [HideInInspector]
        public List<OriginalGameObjectWithMesh> originalGosToRestore = new List<OriginalGameObjectWithMesh>();
        [HideInInspector]
        public string pathsOfAssetToDelete;
        [HideInInspector]
        public bool thisIsPrefab = false;

        //The UI of this component
        #region INTERFACE_CODE
        [UnityEditor.CustomEditor(typeof(CombinedMeshesManager))]
        public class CustomInspector : UnityEditor.Editor
        {
            //Private temp variables
            Vector2 scrollviewMaterials = Vector2.zero;

            public bool DisableGizmosInSceneView(string scriptClassNameToDisable, bool isGizmosDisabled)
            {
                /*
                *  This method disables Gizmos in scene view, for this component
                */

                if (isGizmosDisabled == true)
                    return true;

                //Try to disable
                try
                {
                    //Get all data of Unity Gizmos manager window
                    var Annotation = System.Type.GetType("UnityEditor.Annotation, UnityEditor");
                    var ClassId = Annotation.GetField("classID");
                    var ScriptClass = Annotation.GetField("scriptClass");
                    var Flags = Annotation.GetField("flags");
                    var IconEnabled = Annotation.GetField("iconEnabled");

                    System.Type AnnotationUtility = System.Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
                    var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    //Scann all Gizmos of Unity, of this project
                    System.Array annotations = (System.Array)GetAnnotations.Invoke(null, null);
                    foreach (var a in annotations)
                    {
                        int classId = (int)ClassId.GetValue(a);
                        string scriptClass = (string)ScriptClass.GetValue(a);
                        int flags = (int)Flags.GetValue(a);
                        int iconEnabled = (int)IconEnabled.GetValue(a);

                        // this is done to ignore any built in types
                        if (string.IsNullOrEmpty(scriptClass))
                        {
                            continue;
                        }

                        const int HasIcon = 1;
                        bool hasIconFlag = (flags & HasIcon) == HasIcon;

                        //If the current gizmo is of the class desired, disable the gizmo in scene
                        if (scriptClass == scriptClassNameToDisable)
                        {
                            if (hasIconFlag && (iconEnabled != 0))
                            {
                                /*UnityEngine.Debug.LogWarning(string.Format("Script:'{0}' is not ment to show its icon in the scene view and will auto hide now. " +
                                    "Icon auto hide is checked on script recompile, if you'd like to change this please remove it from the config", scriptClass));*/
                                SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
                            }
                        }
                    }

                    return true;
                }
                //Catch any error
                catch (System.Exception exception)
                {
                    string exceptionOcurred = "";
                    exceptionOcurred = exception.Message;
                    if (exceptionOcurred != null)
                        exceptionOcurred = "";
                    return false;
                }
            }

            public Rect GetInspectorWindowSize()
            {
                //Returns the current size of inspector window
                return EditorGUILayout.GetControlRect(true, 0f);
            }

            public override void OnInspectorGUI()
            {
                //Start the undo event support, draw default inspector and monitor of changes
                DrawDefaultInspector();
                CombinedMeshesManager script = (CombinedMeshesManager)target;
                EditorGUI.BeginChangeCheck();
                Undo.RecordObject(target, "Undo Event");
                script.gizmosOfThisComponentIsDisabled = DisableGizmosInSceneView("CombinedMeshesManager", script.gizmosOfThisComponentIsDisabled);

                //Start of UI
                EditorGUILayout.HelpBox("This GameObject contains the meshes you previously combined. See below, what each option does.\n\n" +
                    "Select Original Meshes - Selects all original Meshes/Gameobjects that are linked to this merge.\n\n" +
                    "Undo And Delete This Merge - It will undo this merge, and will delete this GameObject. All original meshes will be restored to their original state automatically (if possible)."
                    , MessageType.Info);

                GUILayout.Space(20);

                //Verify if has missing files of merge, if data save in assets option is enabled
                if (script.pathsOfAssetToDelete != "")
                {
                    MeshFilter mergedMesh = script.GetComponent<MeshFilter>();
                    if (mergedMesh.sharedMesh == null)
                    {
                        EditorGUILayout.HelpBox("Oops! It looks like there are missing mesh files in this merge. To solve this problem, you can undo this merge and re-do it again!", MessageType.Error);
                        GUILayout.Space(20);
                    }
                }

                //If this merge is a prefab, a copy of original, not renderizes the management buttons
                if (script.thisIsPrefab == false || script.originalGosToRestore.Count > 0)
                {
                    //Select all original gameObjects
                    EditorGUILayout.LabelField("Selection Of Original GameObjects", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    if (GUILayout.Button("Select All Original GameObjects!", GUILayout.Height(30)))
                    {
                        List<GameObject> gameObjects = new List<GameObject>();
                        foreach (OriginalGameObjectWithMesh ogo in script.originalGosToRestore)
                            gameObjects.Add(ogo.gameObject);
                        Selection.objects = gameObjects.ToArray();
                    }

                    //Select all original gameObjects with X material
                    Dictionary<Material, List<GameObject>> objects = new Dictionary<Material, List<GameObject>>();
                    foreach (OriginalGameObjectWithMesh oGo in script.originalGosToRestore)
                    {
                        foreach (Material mat in oGo.meshRenderer.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                if (objects.ContainsKey(mat) == false)
                                    objects.Add(mat, new List<GameObject>() { oGo.gameObject });
                                if (objects.ContainsKey(mat) == true)
                                    objects[mat].Add(oGo.gameObject);
                            }
                        }
                    }

                    //Create a scroll view to select all gameobjects where material is equal to...
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Selection By Material", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Select All Original Meshes That Uses...", GUILayout.Width(320));
                    GUILayout.Space(GetInspectorWindowSize().x - 320);
                    EditorGUILayout.LabelField("Size", GUILayout.Width(30));
                    EditorGUILayout.IntField(objects.Keys.Count, GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    GUILayout.BeginVertical("box");
                    scrollviewMaterials = EditorGUILayout.BeginScrollView(scrollviewMaterials, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(GetInspectorWindowSize().x), GUILayout.Height(150));
                    if (objects.Keys.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Oops! The original materials of this blend were not found!", MessageType.Info);
                    }
                    if (objects.Keys.Count > 0)
                    {
                        foreach (var key in objects.Keys)
                        {
                            if (GUILayout.Button("\"" + key.name + "\" Material", GUILayout.Height(24)))
                                Selection.objects = objects[key].ToArray();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                    GUILayout.EndVertical();

                    //Undo and delete this merge
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Management Of This Merge", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    if (GUILayout.Button("Undo And Delete This Merge", GUILayout.Height(30)))
                    {
                        bool confirmation = EditorUtility.DisplayDialog("Undo",
                            "This combined mesh and your GameObject will be deleted and removed from your scene. The original GameObjects/Meshes will be restored to their original state before the merge.\n\nAre you sure you want to undo this merge?",
                            "Yes",
                            "No");
                        if (confirmation == true)
                            script.UndoAndDeleteThisMerge();
                    }
                }
                if (script.thisIsPrefab == true && script.originalGosToRestore.Count == 0)
                {
                    EditorGUILayout.HelpBox("This merge is a Prefab of the original merge. If you want to manage the merge, please go to the original resulting merge, the merge that was generated when you first combined these meshes. If you no longer have it, and you want to undo this merge, you can just delete this GameObject, but this will not automatically re-activate the original meshes, since the Prefabs of the mesh resulting from the merging lose references to the original meshes.", MessageType.Warning);
                }

                //Final space
                GUILayout.Space(10);

                //Stop paint of GUI, if this gameobject no more exists
                if (script == null)
                {
                    return;
                }

                //Apply changes on script, case is not playing in editor
                if (GUI.changed == true && Application.isPlaying == false)
                {
                    EditorUtility.SetDirty(script);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
                }
                if (EditorGUI.EndChangeCheck() == true)
                {

                }
            }
        }
        #endregion

        //Component code
        void UndoAndDeleteThisMerge()
        {
            //Undo the merge according the type of merge
            if (undoMethod == UndoMethod.EnableOriginalMeshes)
            {
                foreach (OriginalGameObjectWithMesh original in originalGosToRestore)
                {
                    //Skip, if is null
                    if (original.meshRenderer == null)
                    {
                        continue;
                    }
                    original.meshRenderer.enabled = original.originalMrState;
                }
            }
            if (undoMethod == UndoMethod.ReactiveOriginalGameObjects)
            {
                foreach (OriginalGameObjectWithMesh original in originalGosToRestore)
                {
                    //Skip, if is null
                    if (original.gameObject == null)
                    {
                        continue;
                    }
                    original.gameObject.SetActive(original.originalGoState);
                }
            }

            //Delete unused asset, if this is not a prefab
            if (thisIsPrefab == false)
            {
                if (AssetDatabase.LoadAssetAtPath(pathsOfAssetToDelete, typeof(Mesh)) != null)
                {
                    AssetDatabase.DeleteAsset(pathsOfAssetToDelete);
                }
            }

            //Set scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            //Show dialog
            EditorUtility.DisplayDialog("Undo Merge", "The merge was successfully undone. All of the original GameObject/Meshes that this Manager could still access have been restored!\n\nIf you had chosen to save the merged meshes to your project files, all useless mesh files were deleted automatically!", "Ok");

            //Destroy this merge
            DestroyImmediate(this.gameObject, true);
        }
#endif
    }
}