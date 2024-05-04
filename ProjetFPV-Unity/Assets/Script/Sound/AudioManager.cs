using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;


namespace MyAudio
{
    public class AudioManager : GenericSingletonClass<AudioManager>
    {
        public List<AudioSO> audioSO;
        public MyAudioSource PrefabAudioSource;

        [SerializeField] private AudioMixer audioMixer;
        private void Start()
        {
            SpawnAudio(transform.position, SfxType.Music, 0, .5f, 1, 0);
        }

        public void SpawnAudio(Vector3 position, SfxType sfxType, int audioID, float volume, float pitch, float spatialBlend, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, position, Quaternion.identity);

            //audioSource.aSource.volume = volume;
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(volume, 1);
            audioSource.aSource.pitch = pitch;
            audioSource.aSource.spatialBlend = spatialBlend;
            audioSource.aSource.loop = loop;
            audioSource.aSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(sfxType.ToString())[0];
             
            foreach (var soundClip in audioSO[(int)sfxType].soundList)
            {
                if (audioID == soundClip.audioId)
                {
                    audioSource.aSource.clip = audioSO[(int)sfxType].soundList[audioID].audioClip;
                    audioSource.timeBeforeDestroy = audioSO[(int)sfxType].soundList[audioID].audioDuration;
                    audioSource.gameObject.name = audioSO[(int)sfxType].soundList[audioID].soundName;
                }
            }
            
            audioSource.aSource.Play();
        }
    }

    public enum SfxType
    {
        SFX,
        Music,
        Ambiance
    }

    [System.Serializable]
    public class Sound
    {
        public int audioId;
        public string soundName;
        public float audioDuration;
        public AudioClip audioClip;
    }
}



