using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyAudio;
using Script;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : GenericSingletonClass<PauseMenu>, IDestroyInstance
{
    [SerializeField] private GameObject[] containers;
    [SerializeField] private TextMeshProUGUI[] volumesTexts;
    
    private Canvas _canvas;
    private Animator _animator;

    private bool canClose = true;
    private bool canOpen = true;

    public bool isMenuOpened;
    public Button cheatButton;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _animator = GetComponent<Animator>();
            
        _canvas.enabled = false;
        
        SetTextsDefaults();
    }

    private IEnumerator InitPause()
    {
        if (FindObjectOfType<GameIntroduction>())
        {
            if (FindObjectOfType<GameIntroduction>().isInIntro)
            {
                yield break;
            }
        }
        
        isMenuOpened = true;
        
        _canvas.enabled = true;
        _animator.SetTrigger("Open");
        
        Time.timeScale = 0f;

        canClose = false;
        canOpen = false;
        
        PostProcessCrossFade.Instance.CrossFadeTo(1);
        
        SetCursorState(true);
        EnableContainer(0);
        
        PlayerInputs.Instance.EnablePlayerInputs(false);
        
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 53, 1,0,1);

        yield return new WaitForSecondsRealtime(2f);

        canClose = true;
    }

    public void QuitPause()
    {
        StartCoroutine(nameof(QuitPauseRoutine));
    }

    
    IEnumerator QuitPauseRoutine()
    {
        canOpen = false;
        canClose = false;
        
        _animator.SetTrigger("Close");
        SetCursorState(false);
        
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 54, 1,0,1);
        
        PostProcessCrossFade.Instance.CrossFadeTo(0);

        yield return new WaitForSecondsRealtime(1f);
        
        isMenuOpened = false;
        
        _canvas.enabled = false;
        
        Time.timeScale = 1f;
        
        EnableContainer(0);
        
        PlayerInputs.Instance.EnablePlayerInputs(true);

        yield return new WaitForSecondsRealtime(2f);
        
        canOpen = true;
    }

    public void QuitOptions()
    {
        EnableContainer(0);
        //AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 54, 1,0,1);
    }

    public void OpenOptions()
    {
        EnableContainer(1);
        //AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 53, 1,0,1);
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
    
    public void SetGodMod()
    {
        OptionsDDOL.Instance.SetFullscreen();
        
        if (OptionsDDOL.Instance.isInGodMod)
        {
            cheatButton.GetComponent<Image>().DOColor(new Color(0.1296512f, 0.1803922f, 0.1254902f, 1f), 0.1f).SetUpdate(true);
        }
        else
        {
            cheatButton.GetComponent<Image>().DOColor(new Color(0.1803922f, 0.1404086f, 0.1254902f, 1f), 0.1f).SetUpdate(true);
        }
    }

    private void SetTextsDefaults()
    {
        volumesTexts[0].text = Mathf.RoundToInt(100) + "%";
        volumesTexts[1].text = Mathf.RoundToInt(100) + "%";
        volumesTexts[2].text = Mathf.RoundToInt(100) + "%";
        volumesTexts[3].text = $"x{OptionsDDOL.Instance.sensibility:F1}";
    }

    public void BackToMenu()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!UpgradeModule.Instance) return;
            if (!UpgradeModule.Instance.interaction.alreadyInteracted)
            {
                if (!isMenuOpened && canOpen)
                {
                    StartCoroutine(InitPause());
                }
                else if(canClose)
                {
                    QuitPause();
                }
            }
        }
    }

    public void Kill()
    {
        PlayerHealth.Instance.Hit(999);
    }
    
    public void DestroyInstance()
    {
        Destroy(gameObject);
    }
}
