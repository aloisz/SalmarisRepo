using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyAudio;
using NaughtyAttributes;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Weapon;
using Random = UnityEngine.Random;

public class UpgradeModule : GenericSingletonClass<UpgradeModule>
{
    [SerializeField][CurveRange(0,0,1,1, EColor.Orange)] private AnimationCurve landingCurve;
    [SerializeField] private float landingDuration;

    [SerializeField][CurveRange(0,0,1,1, EColor.Orange)] private AnimationCurve leaveCurve;
    [SerializeField] private float leaveDuration;
    
    [SerializeField] private float fullRotateAmount;
    
    [SerializeField] private float offsetLandingY;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float anticipationRaycastLenght;
    
    [SerializeField] private Canvas upgradeMenu;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform upgradeOffersTransform;
    [SerializeField] private int upgradeOffersAmount;
    [SerializeField] UpgradeButton upgradeButtonReference;
    [SerializeField] Transform keyboard;
    [SerializeField] Vector3 keyboardOffset;

    private RaycastHit _hitGroundLanding;
    private Vector3 orbitPosition;
    private Vector3 _baseScale;
    private List<SO_WeaponMode> _currentAvailableUpgrades = new List<SO_WeaponMode>();

    private bool _alreadyland;
    private Vector3 baseKeyboardPosition;

    public Action onUpgrade;
    public UpgradeModuleInteraction interaction;
    
    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _baseScale = transform.localScale;
        transform.localScale = Vector3.zero;

        baseKeyboardPosition = keyboard.transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HUD.Instance.Interact(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HUD.Instance.Interact(false);
        }
    }

    public IEnumerator InitModule(Vector3 position, List<SO_WeaponMode> list, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        
        UpgradeModuleVFX.Instance.StartLanding();
        
        _alreadyland = false;

        orbitPosition = position;
        var t = transform;
        t.position = orbitPosition;
        t.localScale = Vector3.zero;
        
        CheckGroundLandingPosition();
        StartCoroutine(nameof(LeaveAfterIdle));
        
        if (_hitGroundLanding.collider is null) 
            throw new Exception("Cannot land the module because it can't found the ground.");

        // Audio
        AudioManager.Instance.SpawnAudio3D(transform, SfxType.SFX, 23, 1,0,1,1, 0,
            AudioRolloffMode.Linear, 30,100);
        
        t.DOMove(_hitGroundLanding.point + new Vector3(0, offsetLandingY, 0),
            landingDuration).SetEase(landingCurve).SetUpdate(true).OnComplete(() =>
        {
            keyboard.DOLocalMove(baseKeyboardPosition + new Vector3(keyboardOffset.x, keyboardOffset.y, keyboardOffset.z),
                1f).SetUpdate(true).OnComplete((() => 
                AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 25, 1,0,1, 1, 0,
                    AudioRolloffMode.Linear, 5,100)));
        });
        
        t.DOScale(_baseScale, landingDuration).SetEase(landingCurve).SetUpdate(true);
        t.DORotate(new Vector3(0, 360f * fullRotateAmount, 0), landingDuration, 
            RotateMode.FastBeyond360).SetEase(landingCurve).SetUpdate(true);

        _currentAvailableUpgrades = list;
    }

    private void Update()
    {
        if (_hitGroundLanding.collider is null) return;

        if (Vector3.Distance(_hitGroundLanding.point + new Vector3(0, offsetLandingY, 0), transform.position) < 2f && !_alreadyland)
        {
            UpgradeModuleVFX.Instance.LandVFX();
            _alreadyland = true;
            AudioManager.Instance.SpawnAudio3D(transform, SfxType.SFX, 24, 1, 0, 1,1, 0,
                AudioRolloffMode.Linear, 5,100);
        }
    }

    public void LeftModule()
    {
        var t = transform;
        
        t.DOMove(orbitPosition, leaveDuration).SetEase(leaveCurve).SetUpdate(true);
        t.DOScale(Vector3.zero, leaveDuration).SetEase(leaveCurve).SetUpdate(true);
        t.DORotate(new Vector3(0, -360f * fullRotateAmount, 0), leaveDuration, 
            RotateMode.FastBeyond360).SetEase(leaveCurve).SetUpdate(true);
        
        keyboard.DOLocalMove(baseKeyboardPosition, 0.25f).SetUpdate(true);
        
        UpgradeModuleVFX.Instance.GoAwayVFX();

        AudioManager.Instance.SpawnAudio3D(transform.position, SfxType.SFX, 28, 1, 0, 1, 1, 0,
            AudioRolloffMode.Linear, 5, 100);
        MusicManager.Instance.ManageActualSoundVolume(0.25f);
        
        VoicelineManager.Instance.CallShopLeaveVoiceLine();
    }

    private void CheckGroundLandingPosition()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, anticipationRaycastLenght, groundMask);

        // Sort the hits by distance
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Boundaries"))
            {
                // Skip this hit
                continue;
            }

            // If we find a valid hit, set it to _hitGroundLanding and break the loop
            _hitGroundLanding = hit;
            break;
        }
    }

    public void InitMenu()
    {
        upgradeMenu.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        Time.timeScale = 0f;
        
        PostProcessCrossFade.Instance.CrossFadeTo(1);
        
        PlayerHealth.Instance.RestoreHealth(999);
        WeaponState.Instance.barbatos.ResetMunitionWithoutAnim();
        
        PlayerInputs.Instance.EnablePlayerInputs(false);

        PlayerController.Instance.currentActionState = PlayerController.PlayerActionStates.Idle;
        
        GenerateUpgradeOffers();
        
        animator.SetTrigger("Open");
        
        StopAllCoroutines();
        
        // audio
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 26, 1,0,1);
        
        VoicelineManager.Instance.CallShopInVoiceLine();
    }
    
    private void GenerateUpgradeOffers()
    {
        foreach (Transform t in upgradeOffersTransform.GetComponentsInChildren<Transform>())
        {
            if (t != upgradeOffersTransform)
            {
                Destroy(t.gameObject);
            }
        }
        
        StartCoroutine(GetRandomInList(upgradeOffersAmount));

        for (int i = 0; i < upgradeOffersAmount; i++)
        {
            var newObject = Instantiate(upgradeButtonReference, upgradeOffersTransform);
            newObject.InitUpgradeButton(soWeaponModes[i]);
        }
    }

    private List<SO_WeaponMode> soWeaponModes = new List<SO_WeaponMode>();
    private IEnumerator GetRandomInList(int amount)
    {
        soWeaponModes.Clear();
        
        for (int i = 0; i < amount; i++)
        {
            var random = _currentAvailableUpgrades[Random.Range(0, _currentAvailableUpgrades.Count)];
            while(soWeaponModes.Contains(random) || 
                  WeaponState.Instance.barbatos.so_Weapon.weaponMode[0] == random ||
                  WeaponState.Instance.barbatos.so_Weapon.weaponMode[1] == random) 
                random = _currentAvailableUpgrades[Random.Range(0, _currentAvailableUpgrades.Count)];
            
            soWeaponModes.Add(random);
        }

        yield break;
    }
    
    public void QuitMenu()
    {
        StartCoroutine(nameof(QuitMenuRoutine));
        interaction.alreadyInteracted = false;
    }

    private IEnumerator QuitMenuRoutine()
    {
        animator.SetTrigger("Close");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        PostProcessCrossFade.Instance.CrossFadeTo(0);
        
        yield return new WaitForSecondsRealtime(.75f);
        
        upgradeMenu.enabled = false;
        
        Time.timeScale = 1f;
        
        PlayerInputs.Instance.EnablePlayerInputs(true);
        
        LeftModule();
    }

    private IEnumerator LeaveAfterIdle()
    {
        yield return new WaitForSeconds(30f);
        QuitMenu();
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawRay(transform.position, Vector3.down * anticipationRaycastLenght);
        if(_hitGroundLanding.collider) Gizmos.DrawSphere(_hitGroundLanding.point, 0.5f);
    }
}
