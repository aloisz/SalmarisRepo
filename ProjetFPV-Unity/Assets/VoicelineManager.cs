using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
    public TextTag[] customTags;

    private AudioSource _lastSource;

    public void CallVoiceLine(int id)
    {
        StartCoroutine(CallVoiceLineRoutine(id));
    }

    private IEnumerator CallVoiceLineRoutine(int id)
    {
        foreach (IASound iaSound in scriptable.soundList)
        {
            if (iaSound.ID == id)
            {
                if (iaSound.IASoundID.cutPlayingSound)
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
                }
                else
                {
                    yield return new WaitUntil(() => _lastSource == null || !_lastSource.isPlaying);
                    yield return new WaitForSecondsRealtime(1f); //Minimum delay between each voicelines
                }
                
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

                _lastSource = audioSource.aSource;
                
                Subtitle spawnedSubtitle = Instantiate(subtitle, canvas.transform);
                spawnedSubtitle.SetText(iaSound.IASoundID.text);
                if(spawnedSubtitle != null) spawnedSubtitle.DestroySubtitle(iaSound.IASoundID.audioDuration);

                yield break;
            }
        }
    }
    
    public string HighlightCustomTags(string text)
    {
        foreach (var tag in customTags)
        {
            string pattern = $@"<{tag.tag}>(.*?)</{tag.tag}>";
            text = Regex.Replace(text, pattern, match =>
            {
                string content = match.Groups[1].Value;
                string[] words = content.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = $"<color=#{ColorUtility.ToHtmlStringRGB(tag.color)}>{words[i]}</color>";
                }
                return string.Join(" ", words);
            }, RegexOptions.IgnoreCase);
        }
        return text;
    }
}


[Serializable]
public class TextTag
{
    public string tag;
    public Color color;
}
