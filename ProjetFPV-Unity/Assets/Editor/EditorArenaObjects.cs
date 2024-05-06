using System;
using UnityEngine;
using UnityEditor;

public class EditorArenaObjects : Editor
{
    [MenuItem("GameObject/Create Arena", false, -1)]
    private static void CreateArena(MenuCommand menuCommand)
    {
        // Create a new game object
        GameObject newObject = new GameObject();

        //var iconContent = EditorGUIUtility.IconContent("Assets/Editor/Icons/Wave.png");
        //EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);
        
        // Register the creation of the object so it gets properly positioned in the scene
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);

        int i = 0;
        foreach (ArenaTrigger at in FindObjectsOfType<ArenaTrigger>()) i++;
        
        newObject.name = $"Arena_{i}";

        newObject.AddComponent<BoxCollider>();
        newObject.GetComponent<BoxCollider>().size = new Vector3(50, 5, 50);
        newObject.GetComponent<BoxCollider>().isTrigger = true;
            
        newObject.AddComponent<ArenaTrigger>();

        // Register the creation in the undo system, so the user can undo the operation
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
    }
    
    [MenuItem("GameObject/Create Wave", false, -1)]
    private static void CreateBlankWave(MenuCommand menuCommand)
    {
        // Create a new game object
        GameObject newObject = new GameObject();

        var iconContent = EditorGUIUtility.IconContent("Assets/Editor/Icons/Wave.png");
        EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);
        
        // Register the creation of the object so it gets properly positioned in the scene
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);

        newObject.name = $"Wave_{newObject.transform.GetSiblingIndex()}";

        // Register the creation in the undo system, so the user can undo the operation
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
    }
    
    [MenuItem("GameObject/Create Enemy", false, -1)]
    private static void CreateEnemy(MenuCommand menuCommand)
    {
        // Create a new game object
        GameObject newObject = new GameObject();

        var iconContent = EditorGUIUtility.IconContent("Assets/Editor/icons/Enemy.png");
        EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);

        newObject.AddComponent<MobArenaDisplay>();
        
        // Register the creation of the object so it gets properly positioned in the scene
        GameObjectUtility.SetParentAndAlign(newObject, menuCommand.context as GameObject);
        
        newObject.name = $"Enemy_{newObject.transform.GetSiblingIndex()}";

        // Register the creation in the undo system, so the user can undo the operation
        Undo.RegisterCreatedObjectUndo(newObject, "Create " + newObject.name);
    }
}