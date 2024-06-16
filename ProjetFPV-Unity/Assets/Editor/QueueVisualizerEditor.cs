using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(VoicelineManager))]
public class QueueVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object (QueueVisualizer)
        VoicelineManager queueVisualizer = (VoicelineManager)target;

        // Draw the default inspector
        DrawDefaultInspector();

        // Add a space in the inspector
        EditorGUILayout.Space();

        // Add a label for the queue visualization
        EditorGUILayout.LabelField("Queue Visualization", EditorStyles.boldLabel);

        // Visualize the queue
        if (queueVisualizer._audioQueue != null)
        {
            if (queueVisualizer._audioQueue.Count == 0)
            {
                EditorGUILayout.LabelField("Queue is empty.");
            }
            else
            {
                foreach (var item in queueVisualizer._audioQueue)
                {
                    EditorGUILayout.LabelField(item.ToString());
                }
            }
        }

        // Apply any changes made in the inspector
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}