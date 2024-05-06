using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyAudio
{
    [CreateAssetMenu(menuName = "AudioSO/Audio", fileName = "new audio")]
    public class AudioSO : ScriptableObject
    {
        [field: Header("-----Audio-----")]
        [field: SerializeField] public List<Sound> soundList;

    }
}

