using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class OptionsDDOL : GenericSingletonClass<OptionsDDOL>
{
    [SerializeField] private AudioMixer audioMixer;
    public float[] volumes;
    public bool isInFullScreen;
    public float sensibility;
    
    [Header("DO NOT TOUCH")]
    [SerializeField] private Vector2 minMaxAudioValues;
    [SerializeField] private Vector2 minMaxSensibilityValues;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        sensibility = 1f;
    }

    public void SetMixerGroupVolumeMusic(Slider slider)
    {
        var lerp = Mathf.Lerp(minMaxAudioValues.x, minMaxAudioValues.y, slider.value);
        audioMixer.SetFloat("_VolumeMusic", lerp);
        volumes[0] = lerp;
    }
    
    public void SetMixerGroupVolumeSFX(Slider slider)
    {
        var lerp = Mathf.Lerp(minMaxAudioValues.x, minMaxAudioValues.y, slider.value);
        audioMixer.SetFloat("_VolumeSFX", lerp);
        volumes[1] = lerp;
    }
    
    public void SetMixerGroupVolumeAmbiance(Slider slider)
    {
        var lerp = Mathf.Lerp(minMaxAudioValues.x, minMaxAudioValues.y, slider.value);
        audioMixer.SetFloat("_VolumeAmbiance", lerp);
        volumes[2] = lerp;
    }
    
    public void SetSensibility(Slider slider)
    {
        sensibility = Mathf.Lerp(minMaxSensibilityValues.x, minMaxSensibilityValues.y, slider.value);
    }

    public void SetFullscreen(Toggle toggle)
    {
        isInFullScreen = toggle.isOn;
        Screen.fullScreen = toggle.isOn;
        Screen.fullScreenMode = isInFullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    }
}
