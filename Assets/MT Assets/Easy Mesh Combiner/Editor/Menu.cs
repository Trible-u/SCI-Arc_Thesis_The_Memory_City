using UnityEngine;
using UnityEditor;
using System.IO;

namespace MTAssets.EasyMeshCombiner.Editor
{

    /* 
     * This class is responsible for creating the menu for this asset. 
     */

    public class Menu : MonoBehaviour
    {
        //Right click menu items

        [MenuItem("GameObject/Combine Meshes", false, 30)]
        static void OpenMeshCombinerToolWithHierarchy()
        {
            MeshCombinerTool.OpenWindow();
        }

        //Menu items

        [MenuItem("Tools/MT Assets/Easy Mesh Combiner/Mesh Combiner Tool", false, 10)]
        static void OpenMeshCombinerTool()
        {
            MeshCombinerTool.OpenWindow();
        }

        [MenuItem("Tools/MT Assets/Easy Mesh Combiner/Changelog", false, 10)]
        static void OpenChangeLog()
        {
            string filePath = "Assets/MT Assets/Easy Mesh Combiner/List Of Changes.txt";

            if (File.Exists(filePath) == true)
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset)));
            }
            if (File.Exists(filePath) == false)
            {
                EditorUtility.DisplayDialog("Error", "Unable to open file. The file has been deleted, or moved. Please, to correct this problem and avoid future problems with this tool, remove all files from this asset and install it again.", "Ok");
            }
        }

        [MenuItem("Tools/MT Assets/Easy Mesh Combiner/Read Documentation", false, 30)]
        static void ReadDocumentation()
        {
            EditorUtility.DisplayDialog("Read Documentation", "The documentation is located inside the \n\"MT Assets/Easy Mesh Combiner\" folder. Just unzip \"Documentation.zip\" on your desktop and open the \"Documentation.html\" file with your preferred browser.", "Cool!");
        }

        [MenuItem("Tools/MT Assets/Easy Mesh Combiner/More Assets", false, 30)]
        static void MoreAssets()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/40306");
        }

        [MenuItem("Tools/MT Assets/Easy Mesh Combiner/Support", false, 30)]
        static void GetSupport()
        {
            EditorUtility.DisplayDialog("Support", "If you have any questions, problems or want to contact me, just contact me by email (mtassets@windsoft.xyz).", "Got it!");
        }
    }
}