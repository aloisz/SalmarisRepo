using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using MyAudio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Weapon;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI upgradeName;
    [SerializeField] private TextMeshProUGUI upgradeModeIndex;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [SerializeField] private UIParticle uiParticles;
    
    [SerializeField] private Image upgradeIcon;

    private SO_WeaponMode weaponMode;
    private Animator _animator;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(()=>UpgradeWeapon((int)weaponMode.modeIndex, weaponMode));
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        StartCoroutine(nameof(DelayedInteraction));
    }

    IEnumerator DelayedInteraction()
    {
        yield return new WaitForSecondsRealtime(2.7f);
        GetComponent<CanvasGroup>().interactable = true;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mode"></param>
    public void InitUpgradeButton(SO_WeaponMode mode)
    {
        weaponMode = mode;
        
        upgradeName.text = weaponMode.modeName;
        upgradeModeIndex.text = Enum.GetName(typeof(SO_WeaponMode.ShootingModeIndex), weaponMode.modeIndex);
        
        upgradeDescription.text = weaponMode.modeDescription;

        upgradeIcon.sprite = weaponMode.modeIcon;
    }

    private void UpgradeWeapon(int modeIndex, SO_WeaponMode mode)
    {
        WeaponState.Instance.barbatos.so_Weapon.weaponMode[modeIndex] = mode;
        
        // audio
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 27, 1,0,1);
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 28, 1,0,1);
        
        UpgradeModule.Instance.QuitMenu();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiParticles.Stop();
        uiParticles.Play();
        
        _animator.SetTrigger("Hover");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _animator.SetTrigger("UnHover");
    }
}
