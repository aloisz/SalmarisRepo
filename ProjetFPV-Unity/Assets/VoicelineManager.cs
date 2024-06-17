using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AI;
using DG.Tweening;
using MyAudio;
using Script;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoicelineManager : GenericSingletonClass<VoicelineManager>, IDestroyInstance
{
    public SO_IA_Audio scriptable;
    public MyAudioSource PrefabAudioSource;
    public Canvas canvas;
    public Subtitle subtitle;
    public TextTag[] customTags;

    private AudioSource _lastSource;

    public Queue<int> _audioQueue = new Queue<int>();
    public List<int> _audioQueueListMesCouilles = new List<int>();
    
    private bool _isPlaying = false;
    
    public bool _alreadyEncounterShop;
    public bool _alreadyEncounterArena;
    public bool _alreadyBrokeShield;
    public bool _alreadyFirstDied;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void CallVoiceLine(int id)
    {
        if (_audioQueue.Contains(id)) return;

        _audioQueue.Enqueue(id);
        _audioQueueListMesCouilles.Add(id);

        if (!_isPlaying)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    public void PlayPriorityVoiceLine(int id)
    {
        StopCoroutine(ProcessQueue());
        _audioQueue.Clear(); // Clear the queue to interrupt current playback
        _isPlaying = false; // Reset the playing flag

        StartCoroutine(PlayPriority(id));
    }

    private IEnumerator PlayPriority(int id)
    {
        _isPlaying = true;
        if(_lastSource) _lastSource.Stop();

        yield return StartCoroutine(CallVoiceLineRoutine(id));

        _isPlaying = false;

        // Resume the queue after the priority voice line is played
        if (_audioQueue.Count > 0)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        _isPlaying = true;

        while (_audioQueue.Count > 0)
        {
            int id = _audioQueue.Dequeue();
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

                Debug.Log(iaSound.IASoundID.clip.name);

                if(canvas.transform.childCount > 0) Destroy(canvas.transform.GetChild(0).gameObject);
                Subtitle spawnedSubtitle = Instantiate(subtitle, canvas.transform);
                spawnedSubtitle.SetText(iaSound.IASoundID.text);
                if (spawnedSubtitle != null) spawnedSubtitle.DestroySubtitle(iaSound.IASoundID.audioDuration);

                if(_lastSource == null) yield break;
                yield return new WaitUntil(() => _lastSource && !_lastSource.isPlaying);
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

    public void CallFirstArenaDialogues()
    {
        CallVoiceLine(11);
    }

    public void CallShopVoiceLine()
    {
        CallVoiceLine(Random.Range(15,19));
    }

    public void CallFirstShopVoiceLine()
    {
        if (!_alreadyEncounterShop)
        {
            CallVoiceLine(12);
        }
    }
    
    public void CallArenaEndDialogues()
    {
        if(!_alreadyEncounterArena)
        {
            CallVoiceLine(14);
            _alreadyEncounterArena = true;
        }
        else
        {
            var rand = new int[] { 38, 40, 40 };
            CallVoiceLine(rand[Random.Range(0,3)]);
        }
    }
    
    public void CallShopInVoiceLine()
    {
        if (!_alreadyEncounterShop)
        {
            PlayPriorityVoiceLine(13);
            _alreadyEncounterShop = true;
        }
        else
        {
            PlayPriorityVoiceLine(Random.Range(19,20));
        }
    }
    
    public void CallShopLeaveVoiceLine()
    {
        CallVoiceLine(22);
    }
    
    public void CallHoleDeathVoiceLine()
    {
        var rand = new int[] { 26, 51 };
        CallVoiceLine(rand[Random.Range(0,2)]);
    }
    
    public void CallDeathVoiceLine()
    {
        var rand = new int[] { 27, 28, 29 };
        CallVoiceLine(rand[Random.Range(0,4)]);
    }
    
    public void CallDeathVoiceLine2()
    {
        CallVoiceLine(53);
    }
    
    public void CallFirstBrokenShieldVoiceLine()
    {
        if (!_alreadyBrokeShield)
        {
            _alreadyBrokeShield = true;
            CallVoiceLine(31);
        }
    }
    
    public void CallFirstDeathVoiceLine(bool isInHole)
    {
        if (!_alreadyFirstDied)
        {
            _alreadyFirstDied = true;
            CallVoiceLine(32);
        }
        else if(!isInHole)
        {
            CallDeathVoiceLine();
        }
    }
    
    public void CallLowLifeVoiceLine()
    {
        var rand = new int[] { 33, 34, 37 };
        CallVoiceLine(rand[Random.Range(0,3)]);
    }
    
    public void CallKeyVoiceLine()
    {
        CallVoiceLine(42);
    }

    public void CallOpenDoorKeyVoiceLine()
    {
        var rand = new int[] { 45, 46 };
        CallVoiceLine(rand[Random.Range(0,2)]);
    }
    
    public void CallLockedDoorVoiceLine()
    {
        CallVoiceLine(47);
    }
    
    public void CallLevelOneFinishedVoiceLine()
    {
        CallVoiceLine(48);
    }
    
    public void DestroyInstance()
    {
        Destroy(gameObject);
    }
}

[Serializable]
public class TextTag
{
    public string tag;
    public Color color;
}