using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyAudio;
using NaughtyAttributes;
using UnityEngine;

public class MusicManager : GenericSingletonClass<MusicManager>
{
    [ReadOnly][SerializeField] public Music actualMusicPlayed;
    [SerializeField] private List<MyAudioSource> myMusics;
    [SerializeField] private float volume = .35f;
    [SerializeField] private float volumeSwitchDuration = 1.25f;

    private bool _isFadeIn;
    
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        StartCoroutine(InitAllMusic());
        ChangeMusicPlayed(Music.Intro, 0f, 0f);
    }

    private IEnumerator InitAllMusic()
    {
        for (int i = 0; i < AudioManager.Instance.audioSO[1].soundList.Count; i++)
        {
            MyAudioSource audioSource = Instantiate(AudioManager.Instance.PrefabAudioSource, transform.position, Quaternion.identity, transform);

            //audioSource.aSource.volume = volume;
            audioSource.aSource.volume = 0;
            audioSource.aSource.pitch = 1;
            audioSource.aSource.loop = true;
            audioSource.aSource.spatialBlend = 0;
            audioSource.aSource.outputAudioMixerGroup = AudioManager.Instance.audioMixer.FindMatchingGroups(SfxType.Music.ToString())[0];
            
            audioSource.aSource.clip = AudioManager.Instance.audioSO[1].soundList[i].audioClip;
            audioSource.doLoop = true;
            audioSource.gameObject.name = AudioManager.Instance.audioSO[1].soundList[i].soundName;
            
            audioSource.aSource.Play();
            
            myMusics.Add(audioSource);
            yield return null;
        }
    }

    #region MusicDebug

    [Button("Intro")]
    public void Intro()
    {
        ChangeMusicPlayed(Music.Intro, volumeSwitchDuration, volume);
    }
    
    [Button("Start")]
    public void StartMusic()
    {
        ChangeMusicPlayed(Music.Start, volumeSwitchDuration, volume);
    }
    
    [Button("Fight")]
    public void Fight()
    {
        ChangeMusicPlayed(Music.Fight, volumeSwitchDuration, volume);
    }
    
    [Button("Ambiance")]
    public void Ambiance()
    {
        ChangeMusicPlayed(Music.Ambiance, volumeSwitchDuration, volume);
    }
    
    [Button("Shop")]
    public void Shop()
    {
        ChangeMusicPlayed(Music.Shop, volumeSwitchDuration, volume);
    }
    
    [Button("StartFinalFight")]
    public void StartFinalFight()
    {
        ChangeMusicPlayed(Music.StartFinalFight, volumeSwitchDuration, volume);
    }
    
    [Button("FinalFight")]
    public void FinalFight()
    {
        ChangeMusicPlayed(Music.FinalFight, volumeSwitchDuration, volume);
    }
    
    [Button("FinNiveau")]
    public void FinNiveau()
    {
        ChangeMusicPlayed(Music.FinNiveau, volumeSwitchDuration, volume);
    }
    
    [Button("Menu")]
    public void Menu()
    {
        ChangeMusicPlayed(Music.Menu, volumeSwitchDuration, volume);
    }

    #endregion
    
    public void ChangeMusicPlayed(Music newMusic, float timeToTurnOnVolume, float setVolume, float delay = 0f)
    {
        StartCoroutine(ChangeMusicPlayedRoutine(newMusic, timeToTurnOnVolume, setVolume, delay));
    }

    IEnumerator ChangeMusicPlayedRoutine(Music newMusic, float timeToTurnOnVolume, float setVolume, float delay = 0f)
    {
        _isFadeIn = true;
        
        yield return new WaitForSecondsRealtime(delay);
        
        for (int i = 0; i < myMusics.Count; i++)
        {
            if ((int)actualMusicPlayed == i)
            {
                myMusics[i].aSource.DOFade(0, timeToTurnOnVolume * 1.75f).SetUpdate(true);
            }
            if ((int)newMusic == i)
            {
                myMusics[i].aSource.DOFade(setVolume, timeToTurnOnVolume).SetUpdate(true);
            }
        }
        
        actualMusicPlayed = newMusic;

        yield return new WaitForSecondsRealtime(timeToTurnOnVolume * 1.75f);
        
        _isFadeIn = false;
    }

    public void ManageActualSoundVolume(float volume)
    {
        StartCoroutine(ManageActualSoundVolumeRoutine(volume));
    }
    
    private IEnumerator ManageActualSoundVolumeRoutine(float volume)
    {
        yield return new WaitUntil(() => !_isFadeIn);
        myMusics[(int)actualMusicPlayed].aSource.DOFade(volume, 0.5f).SetUpdate(true);
    }
}


public enum Music
{
    Intro,
    Start,
    Fight,
    Ambiance,
    Shop,
    StartFinalFight,
    FinalFight,
    FinNiveau,
    Menu
}
