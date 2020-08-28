using UnityEditor;
using System.IO;

namespace MTAssets.EasyMeshCombiner.Editor
{
    /*
     * This class is responsible for displaying the welcome message when installing this asset.
     */

    [InitializeOnLoad]
    class Greetings
    {
        static Greetings()
        {
            //Run the script after Unity compiles
            EditorApplication.delayCall += Run;
        }

        static void Run()
        {
            string pathOfThisAsset = "Assets/MT Assets/Easy Mesh Combiner/";
            string pathToGreetingsAsset = "Assets/MT Assets/_AssetsData/Greetings/GreetingsData.Emc.ini";
            string pathToPatcherUpdaterFile = "Assets/MT Assets/_AssetsData/Patcher/Editor/PatcherUpdater.cs";
            string pathToPatcherFixerBase = pathOfThisAsset + "Editor/Patch/PatcherFixer.txt";
            string pathToPatcherFixerFile = "Assets/MT Assets/_AssetsData/Patcher/Editor/PatcherFixer.cs";

            CreateBaseDirectoriesIfNotExists();

            //If the greetings file not exists
            if (AssetDatabase.LoadAssetAtPath(pathToGreetingsAsset, typeof(object)) == null)
            {
                //Create a new greetings file
                File.WriteAllText(pathToGreetingsAsset, "Ok");

                //Show greetings and save
                EditorUtility.DisplayDialog("Easy Mesh Combiner was imported!",
                    "The \"Easy Mesh Combiner\" was imported for your project. You should be able to locate it in the directory\n" +
                    "(" + pathOfThisAsset + ").\n\n" +
                    "Remember to read the documentation to learn how to use this asset. To read the documentation, extract the contents of \"Documentation.zip\" inside the\n" +
                    "(" + pathOfThisAsset + ") folder. Then just open the \"Documentation.html\" in your favorite browser.\n\n" +
                    "If you need help, contact me via email (mtassets@windsoft.xyz).",
                    "Cool!");

                AssetDatabase.Refresh();
            }

            //Install the fixer of PatcherUpdater, if this scripts exists
            if (AssetDatabase.LoadAssetAtPath(pathToPatcherUpdaterFile, typeof(object)) != null)
            {
                File.WriteAllText(pathToPatcherFixerFile, File.ReadAllText(pathToPatcherFixerBase));
                AssetDatabase.Refresh();
            }
        }

        public static void CreateBaseDirectoriesIfNotExists()
        {
            //Create the directory to feedbacks folder, of this asset
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets"))
            {
                AssetDatabase.CreateFolder("Assets", "MT Assets");
            }
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
            {
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            }
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Greetings"))
            {
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Greetings");
            }
        }
    }
}