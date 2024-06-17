using UnityEditor;
using UnityEngine;

public class UpdateTextureSettings : EditorWindow
{
    // List of folders or texture types that you want to process
    private static readonly string[] TextureFolders = { "Assets/ASSETS" };

    [MenuItem("Tools/Update Texture Settings")]
    public static void ShowWindow()
    {
        GetWindow<UpdateTextureSettings>("Update Texture Settings");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Update All Textures"))
        {
            UpdateAllTextures();
        }
    }

    private static void UpdateAllTextures()
    {
        // Find all texture assets in the project
        string[] allTexturePaths = AssetDatabase.FindAssets("t:Texture");

        foreach (string guid in allTexturePaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                bool isTexture = IsInFolder(assetPath, TextureFolders);

                if (isTexture)
                {
                    // Enable read/write for textures
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }

                // Optionally, you can exclude lightmaps by checking if the asset path contains "Lightmap"
                if (assetPath.Contains("Lightmap"))
                {
                    // Set any specific settings for lightmaps if needed
                    // For example, we might want to disable read/write for lightmaps
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }
        }

        // Refresh the asset database to apply changes
        AssetDatabase.Refresh();
    }

    // Helper method to check if the asset is in one of the specified folders
    private static bool IsInFolder(string assetPath, string[] folders)
    {
        foreach (var folder in folders)
        {
            if (assetPath.StartsWith(folder))
            {
                return true;
            }
        }
        return false;
    }
}
