using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

namespace MyAudio
{
    public class MyAudioSource : MonoBehaviour
    {
        [SerializeField] internal AudioSource aSource;
        internal float timeBeforeDestroy;
        internal bool doLoop;
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(timeBeforeDestroy);
            if (!doLoop)
            {
                aSource.DOFade(0, .2f).OnComplete((() => Destroy(gameObject)));
            }
        }
    }
}

