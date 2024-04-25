using System;
using UnityEngine;
using UnityEditor;

public class EditorArenaObjects : Editor
{
    [MenuItem("GameObject/Create Wave", false, 0)]
    private static void CreateBlankWave(MenuCommand menuCommand)
    {
        // Create a new game object
        GameObject newObject = new GameObject();

        var iconContent = EditorGUIUtility.IconContent("Assets/Editor/Wave.png");
        EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);
        
        // Register the creation of the object so it gets properly positioned in the scene
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);

        newObject.name = $"Wave_{newObject.transform.GetSiblingIndex()}";

        // Register the creation in the undo system, so the user can undo the operation
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
    }
    
    [MenuItem("GameObject/Create Enemy", false, 0)]
    private static void CreateEnemy(MenuCommand menuCommand)
    {
        // Create a new game object
        GameObject newObject = new GameObject();

        var iconContent = EditorGUIUtility.IconContent("Assets/Editor/Enemy.png");
        EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);

        newObject.AddComponent<MobArenaDisplay>();
        
        // Register the creation of the object so it gets properly positioned in the scene
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);
        
        newObject.name = $"Enemy_{newObject.transform.GetSiblingIndex()}";

        // Register the creation in the undo system, so the user can undo the operation
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
    }
}