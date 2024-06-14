using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using DG.Tweening;
using MyAudio;
using UnityEngine;

public class VoicelineManager : GenericSingletonClass<VoicelineManager>
{
    public SO_IA_Audio scriptable;
    public MyAudioSource PrefabAudioSource;
    public Canvas canvas;
    public Subtitle subtitle;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        CallVoiceLine(0);
        
        yield return new WaitForSeconds(scriptable.soundList[1].IASoundID.audioDuration + 0.5f);
        CallVoiceLine(1);
        
        yield return new WaitForSeconds(scriptable.soundList[2].IASoundID.audioDuration + 0.5f);
        CallVoiceLine(2);
    }

    public void CallVoiceLine(int id)
    {
        foreach (Transform t in canvas.GetComponentsInChildren<Transform>())
        {
            if(t != canvas.transform) Destroy(t.gameObject);
        }
        
        foreach (AudioSource source in GetComponentsInChildren<AudioSource>())
        {
            if (source.isPlaying)
            {
                source.Stop();
                Destroy(source.gameObject);
            }
        }
        
        foreach (IASound iaSound in scriptable.soundList)
        {
            if (iaSound.ID == id)
            {
                MyAudioSource audioSource = Instantiate(PrefabAudioSource, transform.position, Quaternion.identity, transform);
                
                audioSource.aSource.volume = 1;
                audioSource.aSource.pitch = 1f;
                audioSource.aSource.loop = false;
                audioSource.aSource.spatialBlend = 0;
                audioSource.aSource.outputAudioMixerGroup = AudioManager.Instance.audioMixer.FindMatchingGroups("IA")[0];
             
                audioSource.aSource.clip = iaSound.IASoundID.clip;
                audioSource.timeBeforeDestroy = iaSound.IASoundID.audioDuration;
                audioSource.gameObject.name = iaSound.IASoundID.clip.name;
            
                audioSource.aSource.Play();
                
                Subtitle spawnedSubtitle = Instantiate(subtitle, canvas.transform);
                spawnedSubtitle.SetText(iaSound.IASoundID.text);
                if(spawnedSubtitle != null) spawnedSubtitle.DestroySubtitle(iaSound.IASoundID.audioDuration);

                return;
            }
        }
    }
}
