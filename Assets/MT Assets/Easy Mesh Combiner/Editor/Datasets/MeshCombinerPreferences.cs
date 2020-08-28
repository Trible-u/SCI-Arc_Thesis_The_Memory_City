using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.EasyMeshCombiner.Editor
{
    /*
     * This script is the Dataset of the scriptable object "Preferences". This script saves Easy Mesh Combiner preferences.
     */

    public class MeshCombinerPreferences : ScriptableObject
    {
        public enum AfterMerge
        {
            DisableOriginalMeshes,
            DeactiveOriginalGameObjects
        }

        public string projectName;
        public Rect windowPosition;
        public AfterMerge afterMerge = AfterMerge.DisableOriginalMeshes;
        public bool combineChildrens = true;
        public bool combineInactives = false;
        public bool lightmapSupport = false;
        public bool saveMeshInAssets = true;
        public bool savePrefabOfThis = false;
        public bool representLogsInScene = true;
        public string prefabName = "prefab";
        public string nameOfThisMerge = "Combined Meshes";
    }
}