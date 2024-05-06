using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace MyAudio
{
    public class AudioWindow : EditorWindow
    {
        private string soundName = "Enter Name";
        private SfxType sfxType;
        private AudioClip audioClip;

        private int audioID;
        private float audioDuration;
        private bool groupEnabled;


        [MenuItem("Editor/Audio_Window")] // Create a window on Unity
        public static void ShowWindow()
        {
            GetWindow(typeof(AudioWindow)); 
        }

        private void OnGUI() // 
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        
            GUILayout.Space(16);
            audioID = EditorGUILayout.IntField("Audio ID", audioID);
            soundName = EditorGUILayout.TextField("Sound name", soundName);
            sfxType = (SfxType)EditorGUILayout.EnumPopup("Sfx type", sfxType);
            audioDuration = EditorGUILayout.FloatField("Audio Duration", audioDuration);
            audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio source", audioClip, typeof(AudioClip));

            groupEnabled = EditorGUILayout.Toggle("Optional Settings", groupEnabled);
            
            if (groupEnabled)
            {
                
            }
            DisplayBtn();
        }
        
        private void DisplayBtn()
        {
            for (int i = 0; i < 4; i++)
            {
                EditorGUILayout.Space();
            }
            if (GUILayout.Button("Send to AudioManager"))
            {
                Debug.Log($"{soundName}");
                
                Sound soundItem = new Sound()
                {
                    audioId = this.audioID,
                    soundName = this.soundName,
                    audioClip = this.audioClip,
                    audioDuration = this.audioDuration
                };
                
                switch (sfxType)
                {
                    case SfxType.SFX:
                        AudioManager.Instance.audioSO[0].soundList.Add(soundItem);
                        break;
                    case SfxType.Music:
                        AudioManager.Instance.audioSO[1].soundList.Add(soundItem);
                        break;
                    case SfxType.Ambiance:
                        AudioManager.Instance.audioSO[2].soundList.Add(soundItem);
                        break;
                }
            }
        }
        
        
    }
}
