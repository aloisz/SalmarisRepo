using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace MyAudio
{
    public class AudioManager : GenericSingletonClass<AudioManager>
    {
        public List<AudioSO> audioSO;
        public MyAudioSource PrefabAudioSource;

        public AudioMixer audioMixer;
        
        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        public void SpawnAudio2D(Vector3 position, SfxType sfxType, int audioID, float volume, float timeToTurnOnVolume, float pitch, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, position, Quaternion.identity);

            Sound s;
            s = audioSO[(int)sfxType].soundList.Find(x => x.audioId - 1 == audioID);
            audioSource.aSource.clip = audioSO[(int)sfxType].soundList[audioID].audioClip;
            audioSource.timeBeforeDestroy = audioSO[(int)sfxType].soundList[audioID].audioDuration;
            audioSource.gameObject.name = audioSO[(int)sfxType].soundList[audioID].soundName;
            
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(s.audioVolume, timeToTurnOnVolume).SetUpdate(true);
            audioSource.aSource.pitch = Random.Range(s.minMaxPitch.x, s.minMaxPitch.y);
            audioSource.aSource.loop = loop;
            audioSource.aSource.spatialBlend = 0;
            audioSource.aSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(sfxType.ToString())[0];
            
            audioSource.aSource.Play();
        }
        
        public void SpawnAudio3D(Vector3 position, SfxType sfxType, int audioID, float volume, 
            float timeToTurnOnVolume, float pitch, float dopplerLevel = 1, float spread = 0, 
            AudioRolloffMode mode = AudioRolloffMode.Linear, float minDist = 1, float maxDist = 75, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, position, Quaternion.identity);

            Sound s;
            s = audioSO[(int)sfxType].soundList.Find(x => x.audioId - 1 == audioID);
            audioSource.aSource.clip = audioSO[(int)sfxType].soundList[audioID].audioClip;
            audioSource.timeBeforeDestroy = audioSO[(int)sfxType].soundList[audioID].audioDuration;
            audioSource.gameObject.name = audioSO[(int)sfxType].soundList[audioID].soundName;
            
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(s.audioVolume, timeToTurnOnVolume).SetUpdate(true);
            audioSource.aSource.pitch = Random.Range(s.minMaxPitch.x, s.minMaxPitch.y);
            audioSource.aSource.spatialBlend = 1;
            audioSource.aSource.loop = loop;
            audioSource.aSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(sfxType.ToString())[0];
            audioSource.aSource.dopplerLevel = dopplerLevel;
            audioSource.aSource.spread = spread;
            audioSource.aSource.rolloffMode = mode;
            audioSource.aSource.minDistance = minDist;
            audioSource.aSource.maxDistance = maxDist;
            
            audioSource.aSource.Play();
        }
        
        public void SpawnAudio3D(Transform attachPosition, SfxType sfxType, int audioID, float volume, 
            float timeToTurnOnVolume, float pitch, float dopplerLevel = 1, float spread = 0, 
            AudioRolloffMode mode = AudioRolloffMode.Linear, float minDist = 1, float maxDist = 75, bool loop = false)
        {
            MyAudioSource audioSource = Instantiate(PrefabAudioSource, attachPosition.position, Quaternion.identity, attachPosition);
            
            Sound s;
            s = audioSO[(int)sfxType].soundList.Find(x => x.audioId - 1 == audioID);
            audioSource.aSource.clip = audioSO[(int)sfxType].soundList[audioID].audioClip;
            audioSource.timeBeforeDestroy = audioSO[(int)sfxType].soundList[audioID].audioDuration;
            audioSource.gameObject.name = audioSO[(int)sfxType].soundList[audioID].soundName;
            
            audioSource.aSource.volume = 0;
            audioSource.aSource.DOFade(s.audioVolume, timeToTurnOnVolume).SetUpdate(true);
            audioSource.aSource.pitch = Random.Range(s.minMaxPitch.x, s.minMaxPitch.y);
            audioSource.aSource.spatialBlend = 1;
            audioSource.aSource.loop = loop;
            audioSource.aSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(sfxType.ToString())[0];
            audioSource.aSource.dopplerLevel = dopplerLevel;
            audioSource.aSource.spread = spread;
            audioSource.aSource.rolloffMode = mode;
            audioSource.aSource.minDistance = minDist;
            audioSource.aSource.maxDistance = maxDist;
            
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
        [HideInInspector]public int audioId;
        public string soundName;
        public float audioDuration;
        public float audioVolume = 1;
        public Vector2 minMaxPitch = Vector2.one;
        public AudioClip audioClip;
    }
}



