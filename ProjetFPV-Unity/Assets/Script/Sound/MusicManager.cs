using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyAudio;
using UnityEngine;

public class MusicManager : GenericSingletonClass<MusicManager>
{
    [SerializeField] private Music actualMusicPlayed;
    [SerializeField] private List<MyAudioSource> myMusics;
    [SerializeField] private float volume = .35f;
    [SerializeField] private float volumeSwitchDuration = 1.25f;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    
    private void Start()
    {
        StartCoroutine(InitAllMusic());
        ChangeMusicPlayed(Music.Intro, volumeSwitchDuration, volume);
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

    [ContextMenu("Intro")]
    public void Intro()
    {
        ChangeMusicPlayed(Music.Intro, volumeSwitchDuration, volume);
    }
    
    [ContextMenu("Start")]
    public void StartMusic()
    {
        ChangeMusicPlayed(Music.Start, volumeSwitchDuration, volume);
    }
    
    [ContextMenu("Fight")]
    public void Fight()
    {
        ChangeMusicPlayed(Music.Fight, volumeSwitchDuration, volume);
    }
    
    [ContextMenu("Ambiance")]
    public void Ambiance()
    {
        ChangeMusicPlayed(Music.Ambiance, volumeSwitchDuration, volume);
    }
    
    [ContextMenu("Shop")]
    public void Shop()
    {
        ChangeMusicPlayed(Music.Shop, volumeSwitchDuration, volume);
    }

    public void ChangeMusicPlayed(Music newMusic, float timeToTurnOnVolume, float setVolume)
    {
        for (int i = 0; i < myMusics.Count; i++)
        {
            if ((int)actualMusicPlayed == i)
            {
                myMusics[i].aSource.DOFade(0, timeToTurnOnVolume);
            }
            if ((int)newMusic == i)
            {
                myMusics[i].aSource.DOFade(setVolume, timeToTurnOnVolume);
            }
        }
        
        actualMusicPlayed = newMusic;
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
