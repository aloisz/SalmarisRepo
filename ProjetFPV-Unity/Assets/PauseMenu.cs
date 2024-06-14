using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] containers;
    [SerializeField] private TextMeshProUGUI[] volumesTexts;
    
    private Canvas _canvas;
    private Animator _animator;

    public bool isMenuOpened;

    public static PauseMenu instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _animator = GetComponent<Animator>();
            
        _canvas.enabled = false;
        
        SetTextsDefaults();
    }

    public void InitPause()
    {
        isMenuOpened = true;
        
        _canvas.enabled = true;
        _animator.SetTrigger("Open");
        
        Time.timeScale = 0f;
        
        SetCursorState(true);
        EnableContainer(0);
        
        PlayerInputs.Instance.EnablePlayerInputs(false);
    }

    public void QuitPause()
    {
        StartCoroutine(nameof(QuitPauseRoutine));
    }

    IEnumerator QuitPauseRoutine()
    {
        _animator.SetTrigger("Close");

        yield return new WaitForSecondsRealtime(1f);
        
        isMenuOpened = false;
        
        _canvas.enabled = false;
        
        Time.timeScale = 1f;
        
        SetCursorState(false);
        EnableContainer(0);
        
        PlayerInputs.Instance.EnablePlayerInputs(true);
    }

    public void QuitOptions()
    {
        EnableContainer(0);
    }

    public void OpenOptions()
    {
        EnableContainer(1);
    }

    private void SetCursorState(bool enabled)
    {
        if (enabled)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void EnableContainer(int index)
    {
        ResetAllEffects();
        foreach (var obj in containers)
        {
            if(obj == containers[index]) obj.SetActive(true);
            else obj.SetActive(false);
        }
    }

    private void ResetAllEffects()
    {
        foreach (IResetEffect resetEffect in GetComponentsInChildren<IResetEffect>())
        {
            resetEffect.ResetEffect();
        }
    }
    
    public void SetMixerGroupVolumeMusic(Slider slider)
    {
        OptionsDDOL.Instance.SetMixerGroupVolumeMusic(slider);
        volumesTexts[0].text = Mathf.RoundToInt(slider.value * 100).ToString();
    }
    
    public void SetMixerGroupVolumeSFX(Slider slider)
    {
        OptionsDDOL.Instance.SetMixerGroupVolumeSFX(slider);
        volumesTexts[1].text = Mathf.RoundToInt(slider.value * 100).ToString();
    }
    
    public void SetMixerGroupVolumeAmbiance(Slider slider)
    {
        OptionsDDOL.Instance.SetMixerGroupVolumeAmbiance(slider);
        volumesTexts[2].text = Mathf.RoundToInt(slider.value * 100).ToString();
    }
    
    public void SetSensibility(Slider slider)
    {
        OptionsDDOL.Instance.SetSensibility(slider);
        volumesTexts[3].text = $"x{OptionsDDOL.Instance.sensibility:F1}";
    }

    public void SetFullScreenMode(Toggle toggle)
    {
        OptionsDDOL.Instance.SetFullscreen(toggle);
    }

    private void SetTextsDefaults()
    {
        volumesTexts[0].text = Mathf.RoundToInt(100).ToString();
        volumesTexts[1].text = Mathf.RoundToInt(100).ToString();
        volumesTexts[2].text = Mathf.RoundToInt(100).ToString();
        volumesTexts[3].text = $"x{OptionsDDOL.Instance.sensibility:F1}";
    }
}
