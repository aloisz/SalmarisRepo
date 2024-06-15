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

    private Queue<int> _audioQueue = new Queue<int>();
    private bool _isPlaying = false;

    public void CallVoiceLine(int id)
    {
        _audioQueue.Enqueue(id);
        if (!_isPlaying)
        {
            StartCoroutine(PlayNextInQueue());
        }
    }

    private IEnumerator PlayNextInQueue()
    {
        while (_audioQueue.Count > 0)
        {
            int id = _audioQueue.Dequeue();
            yield return new WaitForSecondsRealtime(1f);
            yield return StartCoroutine(CallVoiceLineRoutine(id));
        }
        _isPlaying = false;
    }

    private IEnumerator CallVoiceLineRoutine(int id)
    {
        _isPlaying = true;
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

                _lastSource = audioSource.aSource;
                
                Subtitle spawnedSubtitle = Instantiate(subtitle, canvas.transform);
                spawnedSubtitle.SetText(iaSound.IASoundID.text);
                if (spawnedSubtitle != null) spawnedSubtitle.DestroySubtitle(iaSound.IASoundID.audioDuration);

                yield return new WaitUntil(() => !_lastSource.isPlaying);
                _lastSource = null;
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