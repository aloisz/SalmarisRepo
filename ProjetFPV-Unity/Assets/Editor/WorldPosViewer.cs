using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
public class WorldPosViewer : Editor 
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.EndHorizontal ();
        
        EditorGUILayout.BeginHorizontal ();
        var t = (Transform)target;
        t.position = EditorGUILayout.Vector3Field("World Position", t.position);
        EditorGUILayout.EndHorizontal ();
        
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.Vector4Field("Quaternion Rotation", 
            new Vector4(t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w));
        EditorGUILayout.EndHorizontal ();
        
        EditorGUILayout.BeginHorizontal ();
        EditorGUILayout.IntField("Child Count", t.childCount);
        EditorGUILayout.EndHorizontal ();
    }
}
