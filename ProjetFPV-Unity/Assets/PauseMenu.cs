using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] containers;
    
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

        if (!BlurBackground.instance) throw new Exception("BlurBackground Singleton missing");
        BlurBackground.instance.Blur(true, 0.1f);
    }

    public void QuitPause()
    {
        isMenuOpened = false;
        
        _canvas.enabled = false;
        
        Time.timeScale = 1f;
        
        SetCursorState(false);
        EnableContainer(0);
        
        PlayerInputs.Instance.EnablePlayerInputs(true);
        
        if (!BlurBackground.instance) throw new Exception("BlurBackground Singleton missing");
        BlurBackground.instance.Blur(false, 0.1f);
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
}
