using System;
using System.Collections.Generic;
using CameraBehavior;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Weapon;
using Weapon.Interface;

public class Barbatos : Shotgun
{
    private BarbatosInput barbatosInput;
    [SerializeField] protected List<Transform> vfxPos;
    [SerializeField] protected float falloffDivider = 10f;
    
    private float lastTimeFired_0; // primary
    private float lastTimeFired_1; // secondary
    
    private bool isPrimary;

    private BarbatosShootVFX shootVFX;

    public Action onHitEnemy;

    protected override void Start()
    {
        base.Start();
        
        barbatosInput = GetComponent<BarbatosInput>();
        shootVFX = GetComponent<BarbatosShootVFX>();
        
        WeaponState.Instance.barbatos.OnHudShoot += UpdateLastTimeFired;
    }

    protected override void GetAllInput()
    {
        if (barbatosInput.isReceivingPrimary)
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

        if (barbatosInput.isReceivingSecondary) Secondary();
        
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
        if (hit.transform.GetComponent<Collider>() != null)
        {
            InstantiateBulletImpact(hit);
        }
        if (hit.transform.GetComponent<IDamage>() != null)
        {
            onHitEnemy.Invoke();
            hit.transform.GetComponent<IDamage>().Hit(so_Weapon.weaponMode[(int)actualWeaponModeIndex].bulletDamage 
                / (int)actualWeaponModeIndex == 0 ? (Vector3.Distance(PlayerController.transform.position
                    + PlayerController.transform.forward * 4f, hit.point) / falloffDivider) : 1f);
        }
        
        if(isFirstBulletGone) return;
        if (hit.transform.TryGetComponent<IExplosion>(out IExplosion explosion))
        {
            Debug.Log(hit.transform.name);
            isFirstBulletGone = true;
            explosion.HitScanExplosion(so_Weapon.weaponMode[(int)actualWeaponModeIndex].whoIsTheTarget);
        }

        //CheckTexturesOfHitMesh(hit);
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
        base.Reload();
        if(actualNumberOfBullet == so_Weapon.weaponMode[0].numberOfBullet) return;
        ParticleSystem particle = Instantiate(so_Weapon.weaponMode[(int)actualWeaponModeIndex].reloadParticle,
            vfxPos[2].position, gunBarrelPos.transform.rotation, transform);
        particle.Play();
    }

    private void PlayParticle()
    {
        GenerateCamShakeOnShoot();
        
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
        
        GameObject particle = Pooling.instance.Pop("BulletImpact");
        particle.transform.position = hit.point;
        particle.transform.up = hit.normal;
        Pooling.instance.DelayedDePop("BulletImpact", particle,3);
        
        DecalSpawnerManager.Instance.SpawnDecal(hit.point, hit.normal, "Impact_Shotgun_Decal");
        
        if (so_Weapon.weaponMode[(int)actualWeaponModeIndex].doExplosion)
        {
            GameObject explosion = Pooling.instance.Pop("ExplosionImpact");
            explosion.transform.position = hit.point;
        }
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
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    /*
    private void CheckTexturesOfHitMesh(RaycastHit hit)
    {
        MeshRenderer meshRenderer = hit.collider.GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null) return;

        Shader shader = meshRenderer.material.shader;
        int propertyCount = ShaderUtil.GetPropertyCount(shader);
        List<Texture> textures = new List<Texture>();

        for (int index = 0; index < propertyCount; index++)
        {
            if (ShaderUtil.GetPropertyType(shader, index) == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                textures.Add(meshRenderer.material.GetTexture(ShaderUtil.GetPropertyName(shader, index)));
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

        if (colors.Count != 0)
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
        else
        {
            shootVFX.SpawnShootVFX(1, hit.point, hit.normal);
        }
    }

    private Color32 GetTextureColor(Texture2D tex, Vector2 uv)
    {
        return tex.GetPixelBilinear(uv.x, uv.y);
    }*/
}

