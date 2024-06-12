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
            
        }

        public void SpawnAudio2D(Vector3 position, SfxType sfxType, int audioID, float volume, float timeToTurnOnVolume, float pitch, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, position, Quaternion.identity);

            //audioSource.aSource.volume = volume;
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(volume, timeToTurnOnVolume);
            audioSource.aSource.pitch = pitch;
            audioSource.aSource.loop = loop;
            audioSource.aSource.spatialBlend = 0;
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
        
        public void SpawnAudio3D(Vector3 position, SfxType sfxType, int audioID, float volume, 
            float timeToTurnOnVolume, float pitch, float dopplerLevel = 1, float spread = 0, 
            AudioRolloffMode mode = AudioRolloffMode.Logarithmic, float minDist = 1, float maxDist = 500, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, position, Quaternion.identity);

            //audioSource.aSource.volume = volume;
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(volume, timeToTurnOnVolume);
            audioSource.aSource.pitch = pitch;
            audioSource.aSource.spatialBlend = 1;
            audioSource.aSource.loop = loop;
            audioSource.aSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(sfxType.ToString())[0];
            audioSource.aSource.dopplerLevel = dopplerLevel;
            audioSource.aSource.spread = spread;
            audioSource.aSource.rolloffMode = mode;
            audioSource.aSource.minDistance = minDist;
            audioSource.aSource.maxDistance = maxDist;
             
            foreach (var soundClip in audioSO[(int)sfxType].soundList)
            {
                if (audioID == soundClip.audioId)
                {
                    audioSource.aSource.clip = audioSO[(int)sfxType].soundList[soundClip.audioId - 1].audioClip;
                    audioSource.timeBeforeDestroy = audioSO[(int)sfxType].soundList[soundClip.audioId - 1].audioDuration;
                    audioSource.gameObject.name = audioSO[(int)sfxType].soundList[soundClip.audioId - 1].soundName;
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



