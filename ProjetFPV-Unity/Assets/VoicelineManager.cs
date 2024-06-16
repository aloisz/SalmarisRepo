using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AI;
using DG.Tweening;
using MyAudio;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    private bool _alreadyEncounterShop;
    private bool _alreadyEncounterArena;
    private bool _alreadyBrokeShield;
    private bool _alreadyFirstDied;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void CallVoiceLine(int id)
    {
        if (_audioQueue.Contains(id)) return;
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
            _isPlaying = true;
            
            yield return StartCoroutine(CallVoiceLineRoutine(id));
            yield return new WaitForSeconds(1f);
        }
        _isPlaying = false;
    }

    private IEnumerator CallVoiceLineRoutine(int id)
    {
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
    
    public void CallVoiceLineIntroduction(int id)
    {
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
                
                _lastSource = null;
            }
        }
    }
    
    public IEnumerator CallFirstArenaDialogues()
    {
        for (int i = 11; i < 12; i++)
        {
            CallVoiceLine(i);
            yield return new WaitForSecondsRealtime(scriptable.soundList[i].IASoundID.audioDuration);
        }
    }

    public IEnumerator CallShopVoiceLine()
    {
        if (!_alreadyEncounterShop)
        {
            for (int i = 12; i < 13; i++)
            {
                yield return new WaitForSeconds(1f);
                CallVoiceLine(i);
                yield return new WaitForSecondsRealtime(scriptable.soundList[i].IASoundID.audioDuration);
            }
        }
        else
        {
            CallVoiceLine(Random.Range(15,19));
        }
    }
    
    public IEnumerator CallArenaEndDialogues()
    {
        if(!_alreadyEncounterArena)
        {
            for (int i = 14; i < 15; i++)
            {
                CallVoiceLine(i);
                yield return new WaitForSecondsRealtime(scriptable.soundList[i].IASoundID.audioDuration);
            }
            _alreadyEncounterArena = true;
        }
        else
        {
            var rand = new int[] { 38, 40, 40 };
            CallVoiceLine(rand[Random.Range(0,3)]);
        }
    }
    
    public IEnumerator CallShopInVoiceLine()
    {
        if (!_alreadyEncounterShop)
        {
            CallVoiceLine(13);
            _alreadyEncounterShop = true;
        }
        else
        {
            CallVoiceLine(Random.Range(19,20));
            yield break;
        }
    }
    
    public IEnumerator CallShopLeaveVoiceLine()
    {
        CallVoiceLine(Random.Range(22,24));
        yield break;
    }
    
    public IEnumerator CallHoleDeathVoiceLine()
    {
        var rand = new int[] { 26, 51 };
        CallVoiceLine(rand[Random.Range(0,2)]);
        yield break;
    }
    
    public IEnumerator CallDeathVoiceLine()
    {
        var rand = new int[] { 27, 28, 29 };
        CallVoiceLine(rand[Random.Range(0,4)]);
        yield break;
    }
    
    public IEnumerator CallFirstBrokenShieldVoiceLine()
    {
        if (!_alreadyBrokeShield)
        {
            _alreadyBrokeShield = true;
            CallVoiceLine(31);
            yield break;
        }
    }
    
    public IEnumerator CallFirstDeathVoiceLine(bool isInHole)
    {
        if (!_alreadyFirstDied)
        {
            _alreadyFirstDied = true;
            CallVoiceLine(32);
            yield break;
        }
        else if(!isInHole)
        {
            StartCoroutine(CallDeathVoiceLine());
        }
    }
    
    public IEnumerator CallLowLifeVoiceLine()
    {
        var rand = new int[] { 33, 34, 37 };
        CallVoiceLine(rand[Random.Range(0,3)]);
        yield break;
    }
    
    public IEnumerator CallKeyVoiceLine()
    {
        CallVoiceLine(42);
        yield break;
    }
    
    public IEnumerator CallOpenDoorKeyVoiceLine()
    {
        var rand = new int[] { 45, 46 };
        CallVoiceLine(rand[Random.Range(0,2)]);
        yield break;
    }
    
    public IEnumerator CallLockedDoorVoiceLine()
    {
        CallVoiceLine(47);
        yield break;
    }
    
    public IEnumerator CallLevelOneFinishedVoiceLine()
    {
        CallVoiceLine(48);
        yield break;
    }
}

[Serializable]
public class TextTag
{
    public string tag;
    public Color color;
}