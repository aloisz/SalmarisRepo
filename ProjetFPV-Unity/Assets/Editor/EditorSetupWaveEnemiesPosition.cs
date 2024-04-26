using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArenaTrigger))] // Replace YourScript with the name of your MonoBehaviour script
public class EditorSetupWaveEnemiesPosition : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ArenaTrigger script = (ArenaTrigger)target;

        // Add a custom button
        if (GUILayout.Button("Generate Enemies Position"))
        {
            // Perform the action when the button is clicked
            script.SetupWaveEnemiesPosition();
        }
    }
}