using System;
using System.Collections.Generic;
using AI;
using CameraBehavior;
using DG.Tweening;
using MyAudio;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Weapon;
using Weapon.Interface;
using Random = UnityEngine.Random;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;
    [SerializeField] protected List<Transform> vfxPos;
    
    [SerializeField] protected List<GameObject> turbine = new List<GameObject>();
    private List<Vector3> turbineLocalPosition = new List<Vector3>(); // Save the local position of the turbine
    
    private float lastTimeFired_0; // primary
    private float lastTimeFired_1; // secondary
    
    private bool isPrimary;

    private BarbatosShootVFX shootVFX;

    public Action onHitEnemy;
    public Action onHitEnemyLethal;
    
    public Action OnReload;
    public Action OnReloadEnd;

    protected override void Start()
    {
        base.Start();
        
        barbatosInput = GetComponent<BarbatosInput>();
        shootVFX = GetComponent<BarbatosShootVFX>();
        
        WeaponState.Instance.barbatos.OnHudShoot += UpdateLastTimeFired;
        
        //Turbine
        turbineLocalPosition.Add(turbine[0].transform.localPosition);
        turbineLocalPosition.Add(turbine[1].transform.localPosition);
    }

    protected override void GetAllInput()
    {
        if (barbatosInput.isReceivingPrimary && !barbatosInput.isReceivingSecondary)
        {
            if (actualWeaponModeIndex != WeaponMode.Primary)
            {
                actualWeaponModeIndex = WeaponMode.Primary;
            }
            isFirstBulletGone = false;
            lastTimefired = lastTimeFired_0;
            isPrimary = true;
            ShootingAction();
        }
        else
        {
            isShooting = false;
            //canFire = true;
        }

        if (barbatosInput.isReceivingSecondary && !barbatosInput.isReceivingPrimary) Secondary();
        
        if (barbatosInput.isReceivingReload) Reload();
    }

    public void Secondary()
    {
        if (actualWeaponModeIndex != WeaponMode.Secondary)
        {
            actualWeaponModeIndex = WeaponMode.Secondary;
        }
        lastTimefired = lastTimeFired_1;
        isPrimary = false;
        ShootingAction();
    }
    
    protected override void HitScanLogic(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            InstantiateBulletImpact(hit);
        }
        if (hit.collider.GetComponent<IDamage>() != null)
        {
            var value = 1f;
            if (Vector3.Distance(PlayerController.transform.position, hit.point) < so_Weapon
                    .weaponMode[(int)actualWeaponModeIndex].fallOffStartingDistance)
            {
                value = so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage;
            }
            else
            {
                value = so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage /
                    Vector3.Distance(PlayerController.transform.position +
                                     PlayerController.transform.forward * so_Weapon
                                         .weaponMode[(int)actualWeaponModeIndex].fallOffStartingDistance,
                        hit.point) * so_Weapon.weaponMode[(int)actualWeaponModeIndex].fallOffByDistanceMultiplier;
            }

            var damage = so_Weapon.weaponMode[(int)actualWeaponModeIndex].doFallOffDamage ? Mathf.Clamp(value, 0, so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage)
                : so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage;
            hit.transform.GetComponent<IDamage>().Hit(damage);

            AI_Pawn aiPawn = hit.transform.GetComponent<AI_Pawn>() ?? hit.transform.GetComponentInParent<AI_Pawn>();

            if (aiPawn != null && !aiPawn.isPawnDead)
            {
                if (aiPawn.actualPawnHealth <= 0)
                {
                    onHitEnemyLethal.Invoke();
                }
                else
                {
                    onHitEnemy.Invoke();
                }
            }
        }
        
        if(isFirstBulletGone) return;
        if (hit.transform.TryGetComponent<IExplosion>(out IExplosion explosion))
        {
            isFirstBulletGone = true;
            explosion.HitScanExplosion(so_Weapon.weaponMode[(int)actualWeaponModeIndex].whoIsTheTarget);
        }
        
        CheckTexturesOfHitMesh(hit);
    }

    private void UpdateLastTimeFired()
    {
        if (isPrimary)
        {
            lastTimeFired_0 = lastTimefired;
        }
        else
        {
            lastTimeFired_1 = lastTimefired;
        }

        PlayParticle();
    }

    public override void Reload()
    {
        //Audio
        if(isReloading) return;
        if(actualNumberOfBullet == so_Weapon.weaponMode[0].numberOfBullet) return;
        if(PlayerKillStreak.Instance.isInRageMode) return;
        
        base.Reload();
        
        OnReload.Invoke();
        
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 37, 1,0,1,false);
        
        if(actualNumberOfBullet == so_Weapon.weaponMode[0].numberOfBullet) return;
        ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].reloadParticle,
            vfxPos[2].position, gunBarrelPos.transform.rotation, transform);
        particle.Play();
        
        //Turbine
        turbine[0].transform.DOLocalMove(turbine[2].transform.localPosition, .1f);
        turbine[1].transform.DOLocalMove(turbine[2].transform.localPosition, .1f);
        
        turbine[0].transform.DOLocalRotate(new Vector3(0,0,100000), so_Weapon.weaponMode[0].timeToReload, RotateMode.LocalAxisAdd);
        turbine[1].transform.DOLocalRotate(new Vector3(0,0,100000), so_Weapon.weaponMode[0].timeToReload, RotateMode.LocalAxisAdd);
    }

    private void PlayParticle()
    {
        // shake
        GenerateCamShakeOnShoot();
        //Audio
        float randomPitch = Random.Range(0.95f, 1.95f);
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, isPrimary ? 39 : 40, 1, 0, randomPitch,
            false);
        
        // particle
        if(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle == null) return;

        if (isPrimary)
        {
            ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle,
                vfxPos[0].position, gunBarrelPos.transform.rotation, transform);
            particle.Play();
        }
        else
        {
            ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].weaponParticle,
                vfxPos[1].position, gunBarrelPos.transform.rotation, transform);
            particle.Play();
        }
        
        turbine[0].transform.DOLocalRotate(new Vector3(0,0,1000), 1, RotateMode.LocalAxisAdd);
        turbine[1].transform.DOLocalRotate(new Vector3(0,0,1000), 1, RotateMode.LocalAxisAdd);
    }

    protected override void EndReload()
    {
        base.EndReload();
        //Audio
        AudioManager.Instance.SpawnAudio2D(transform.position, SfxType.SFX, 38, 1,0,1,false);
        
        OnReloadEnd.Invoke();

        //Turbine
        turbine[0].transform.DOLocalMove(turbineLocalPosition[0], .1f);
        turbine[1].transform.DOLocalMove(turbineLocalPosition[1], .1f);
    }
    
    
    internal BarbatwoBullet bulletProjectile;
    protected override void ShootProjectile()
    {
        base.ShootProjectile();
        bulletProjectile = bulletProjectileGO.GetComponent<BarbatwoBullet>();
        
        // Logic
        bulletProjectile.EnableMovement(true);  
        bulletProjectile.transform.rotation *= Quaternion.AngleAxis(90, PlayerController.transform.right);
        bulletProjectile.UseGravity(true);
        bulletProjectile.GravityApplied(so_Weapon.weaponMode[(int)actualWeaponModeIndex].gravityApplied);
        bulletProjectile.AddVelocity(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletSpeed + PlayerController.direction.magnitude);
        bulletProjectile.AddDamage(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage);
        bulletProjectile.PoolingKeyName(so_Weapon.weaponMode[(int)actualWeaponModeIndex].poolingPopKey);
        
        bulletProjectile.DragModification(so_Weapon.weaponMode[(int)actualWeaponModeIndex].dragApply);
        bulletProjectile.RocketJumpForceApplied(so_Weapon.weaponMode[(int)actualWeaponModeIndex].rocketJumpForceApplied);
        bulletProjectile.WhoIsTheTarget(so_Weapon.weaponMode[(int)actualWeaponModeIndex].whoIsTheTarget);

        switch (so_Weapon.weaponMode[(int)actualWeaponModeIndex].projectileType)
        {
            case ProjectileType.Simple:
                bulletProjectile.DoBounce(ProjectileType.Simple, 0);
                break;
            case ProjectileType.Bounce:
                bulletProjectile.DoBounce(ProjectileType.Bounce, so_Weapon.weaponMode[(int)actualWeaponModeIndex].bounceNbr);
                break;
        }
        
        bulletProjectile.SetTheBulletDir(Vector3.zero);
        bulletProjectile.SetTheBulletDir(GetTheAimDirection() );
    }


    public override void SwapWeapon()
    {
        if (actualNumberOfBullet <= 0)
        {
            Reload();
        }
    }
    
    public override void InstantiateBulletImpact(RaycastHit hit)
    {
        base.InstantiateBulletImpact(hit);
        
        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion = Pooling.Instance.Pop("ExplosionImpact");
            explosion.transform.position = hit.point;
        }
        
        if(hit.collider.GetComponent<AI_Pawn>()) return;
        
        GameObject particle = Pooling.Instance.Pop("BulletImpact");
        particle.transform.position = hit.point;
        particle.transform.up = hit.normal;
        Pooling.Instance.DelayedDePop("BulletImpact", particle,3);
        
        //DecalSpawnerManager.Instance.SpawnDecal(hit.point, hit.normal, "Impact_Shotgun_Decal");
    }

    protected override void WichTypeMunitionIsGettingShot()
    {
        base.WichTypeMunitionIsGettingShot();
        OnLooseAmmo.Invoke();
    }
    
    void GenerateCamShakeOnShoot()
    {
        var mode = so_Weapon.weaponMode[(int)actualWeaponModeIndex];
        switch (actualWeaponModeIndex)
        {
            case WeaponMode.Primary:
                
                CameraShake.Instance.ShakeCamera(mode.shakeDuration, mode.shakeMagnitude, mode.shakeMagnitudeRot, mode.shakeMagnitudeFrequency, 
                    mode.shakeApplyFadeOut, mode.shakePower);
                
                break;
            case WeaponMode.Secondary:
                CameraShake.Instance.ShakeCamera(mode.shakeDuration, mode.shakeMagnitude, mode.shakeMagnitudeRot, mode.shakeMagnitudeFrequency, 
                    mode.shakeApplyFadeOut, mode.shakePower);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    
    private void CheckTexturesOfHitMesh(RaycastHit hit)
    {
        MeshRenderer meshRenderer = hit.collider.GetComponentInChildren<MeshRenderer>();
        SkinnedMeshRenderer skinnedMeshRenderer = hit.collider.GetComponentInChildren<SkinnedMeshRenderer>();

        if (meshRenderer == null)
        {
            if (skinnedMeshRenderer == null)
            {
                return;
            }
        }

        Shader shader = meshRenderer ? meshRenderer.material.shader : skinnedMeshRenderer.material.shader;
        int propertyCount = shader.GetPropertyCount();
        List<Texture> textures = new List<Texture>();
        
        for (int index = 0; index < propertyCount; index++)
        {
            if (shader.GetPropertyType(index) == ShaderPropertyType.Texture)
            {
                textures.Add(meshRenderer ? meshRenderer.material.GetTexture(shader.GetPropertyName(index)) : skinnedMeshRenderer.material.GetTexture(shader.GetPropertyName(index)));
            }
        }

        List<Color32> colors = new List<Color32>();
        Vector2 textureCoord = hit.textureCoord;

        foreach (Texture tex in textures)
        {
            if (tex is Texture2D tex2D)
            {
                if(tex.isReadable) colors.Add(GetTextureColor(tex2D, textureCoord));
            }
        }

        if (colors.Count != 0 && !hit.transform.GetComponent<AI_Pawn>())
        {
            float sumR = 0f, sumG = 0f, sumB = 0f;

            foreach (Color32 c in colors)
            {
                sumR += c.r;
                sumG += c.g;
                sumB += c.b;
            }

            Color32 finalValue = new Color32(
                (byte)(sumR / colors.Count), 
                (byte)(sumG / colors.Count), 
                (byte)(sumB / colors.Count), 
                255
            );
            
            shootVFX.SpawnShootVFX(finalValue, hit.point, hit.normal);
        }
        else if(hit.collider.GetComponent<AI_Pawn>())
        {
            shootVFX.SpawnShootVFX(3, hit.point, hit.normal);
        }
        else
        {
            shootVFX.SpawnShootVFX(1, hit.point, hit.normal);
        }
        
        Debug.DrawRay(hit.point, hit.normal * 5f, Color.green, 10f);
    }

    private Color32 GetTextureColor(Texture2D tex, Vector2 uv)
    {
        return tex.GetPixelBilinear(uv.x, uv.y);
    }

    public void ResetMunitionWithoutAnim()
    {
        actualNumberOfBullet = so_Weapon.weaponMode[0].numberOfBullet;
    }
}

