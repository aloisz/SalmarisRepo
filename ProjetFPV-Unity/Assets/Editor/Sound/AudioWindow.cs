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
        private AudioSource audioSource;
        
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
            soundName = EditorGUILayout.TextField("Sound name", soundName);
            sfxType = (SfxType)EditorGUILayout.EnumPopup("Sfx type", sfxType);
            audioSource = (AudioSource)EditorGUILayout.ObjectField("Audio source", audioSource, typeof(AudioSource));

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
                
                SoundList soundList = new SoundList()
                {
                    sfxType = SfxType.Ambiance,
                    sound = new Sound()
                    {
                        audioId = 1,
                        audioClip = audioSource,
                        soundName = soundName
                    }
                };
                /*soundList.sfxType = SfxType.Ambiance;
                soundList.sound[0].soundName = soundName;
                soundList.sound[0].audioId = 125;*/
                
                AudioManager.Instance.soundList.Add(soundList);
            }
        }
        
        
    }
}
