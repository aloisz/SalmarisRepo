using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MyAudio
{
    public class AudioManager : GenericSingletonClass<AudioManager>
    {
        public List<SoundList> soundList;
        public AudioSource audioToSpawn;
    }

    public enum SfxType
    {
        SFX,
        Music,
        Ambiance
    }

    [System.Serializable]
    public class SoundList
    {
        public SfxType sfxType;
        public Sound sound;
    }

    [System.Serializable]
    public class Sound
    {
        public string soundName;
        public AudioSource audioClip;
        public int audioId;
    }
}



