﻿using System.Collections.Generic;
using Core.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
    }

    [System.Serializable]
    public struct CrosshairData
    {
        [Tooltip("The image that will be used for this weapon's crosshair")]
        public Sprite CrosshairSprite;

        [Tooltip("The size of the crosshair image")]
        public int CrosshairSize;

        [Tooltip("The color of the crosshair image")]
        public Color CrosshairColor;
    }

    [RequireComponent(typeof(AudioSource))]
    public class WeaponController: MonoBehaviour
    {
        public string WeaponName;
        public Sprite WeaponIcon;
        public CrosshairData CrosshairDataDefault;
        public CrosshairData CrosshairDataTargetInSight;
        public GameObject WeaponRoot;
        public Transform WeaponMuzzle;
        public WeaponShootType ShootType;
        public ProjectileBase ProjectilePrefab;
        public float DelayBetweenShots = 0.5f;
        public float BulletSpreadAngle = 0f;
        public int BulletsPerShot = 1;
        public float RecoilForce = 1;
        public float AimZoomRatio = 1;
        public Vector3 AimOffset;
        
        public bool AutomaticReload = true;
        public bool HasPhysicalBullets = false;
        public int ClipSize = 30;
        public GameObject ShellCasing;
        public Transform EjectionPort;
        public float AmmoReloadRate = 1f;
        public float AmmoReloadDelay = 2f;

        public int MaxAmmo = 8;

        public bool AutomaticReleaseOnCharged;

        public float MaxChargeDuration = 2f;

public float AmmoUsedOnStartCharge = 1f;

public float AmmoUsageRateWhileCharging = 1f;

public Animator WeaponAnimator;

public GameObject MuzzleFlashPrefab;

public bool UnparentMuzzleFlash;

public AudioClip ShootSfx;

public AudioClip ChangeWeaponSfx;

public AudioClip ContinuousShootStartSfx;
public AudioClip ContinuousShootLoopSfx;
public AudioClip ContinuousShootEndSfx;
AudioSource m_ContinuousShootAudioSource = null;
bool m_WantsToShoot = false;

public UnityAction OnShoot;
public event Action OnShootProcessed;

int m_CarriedPhysicalBullets;
float m_CurrentAmmo;
float m_LastTimeShot = Mathf.NegativeInfinity;
public float LastChargeTriggerTimestamp { get; private set; }
Vector3 m_LastMuzzlePosition;

public GameObject Owner { get; set; }
public GameObject SourcePrefab { get; set; }
public bool IsCharging { get; private set; }
public float CurrentAmmoRatio { get; private set; }
public bool IsWeaponActive { get; private set; }
public bool IsCooling { get; private set; }
public float CurrentCharge { get; private set; }
public Vector3 MuzzleWorldVelocity { get; private set; }

public float GetAmmoNeededToShoot() =>
    (ShootType != WeaponShootType.Charge ? 1f : Mathf.Max(1f, AmmoUsedOnStartCharge)) /
    (MaxAmmo * BulletsPerShot);

public int GetCarriedPhysicalBullets() => m_CarriedPhysicalBullets;
public int GetCurrentAmmo() => Mathf.FloorToInt(m_CurrentAmmo);

AudioSource m_ShootAudioSource;

public bool IsReloading { get; private set; }

const string k_AnimAttackParameter = "Attack";

private Queue<Rigidbody> m_PhysicalAmmoPool;

void Awake()
{
    m_CurrentAmmo = MaxAmmo;
    m_CarriedPhysicalBullets = HasPhysicalBullets ? ClipSize : 0;
    m_LastMuzzlePosition = WeaponMuzzle.position;

    m_ShootAudioSource = GetComponent<AudioSource>();
    DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this,
        gameObject);

    if (UseContinuousShootSound)
    {
        m_ContinuousShootAudioSource = gameObject.AddComponent<AudioSource>();
        m_ContinuousShootAudioSource.playOnAwake = false;
        m_ContinuousShootAudioSource.clip = ContinuousShootLoopSfx;
        m_ContinuousShootAudioSource.outputAudioMixerGroup =
            AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.WeaponShoot);
        m_ContinuousShootAudioSource.loop = true;
    }

    if (HasPhysicalBullets)
    {
        m_PhysicalAmmoPool = new Queue<Rigidbody>(ShellPoolSize);

        for (int i = 0; i < ShellPoolSize; i++)
        {
            GameObject shell = Instantiate(ShellCasing, transform);
            shell.SetActive(false);
            m_PhysicalAmmoPool.Enqueue(shell.GetComponent<Rigidbody>());
        }
    }
}

public void AddCarriablePhysicalBullets(int count) => m_CarriedPhysicalBullets = Mathf.Max(m_CarriedPhysicalBullets + count, MaxAmmo);

void ShootShell()
{
    Rigidbody nextShell = m_PhysicalAmmoPool.Dequeue();

    nextShell.transform.position = EjectionPort.transform.position;
    nextShell.transform.rotation = EjectionPort.transform.rotation;
    nextShell.gameObject.SetActive(true);
    nextShell.transform.SetParent(null);
    nextShell.collisionDetectionMode = CollisionDetectionMode.Continuous;
    nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

    m_PhysicalAmmoPool.Enqueue(nextShell);
}

void PlaySFX(AudioClip sfx) => AudioUtility.CreateSFX(sfx, transform.position, AudioUtility.AudioGroups.WeaponShoot, 0.0f);


void Reload()
{
    if (m_CarriedPhysicalBullets > 0)
    {
        m_CurrentAmmo = Mathf.Min(m_CarriedPhysicalBullets, ClipSize);
    }

    IsReloading = false;
}

public void StartReloadAnimation()
{
    if (m_CurrentAmmo < m_CarriedPhysicalBullets)
    {
        GetComponent<Animator>().SetTrigger("Reload");
        IsReloading = true;
    }
}

void Update()
{
    UpdateAmmo();
    UpdateCharge();
    UpdateContinuousShootSound();

    if (Time.deltaTime > 0)
    {
        MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
        m_LastMuzzlePosition = WeaponMuzzle.position;
    }
}

void UpdateAmmo()
{
    if (AutomaticReload && m_LastTimeShot + AmmoReloadDelay < Time.time && m_CurrentAmmo < MaxAmmo && !IsCharging)
    {
        // reloads weapon over time
        m_CurrentAmmo += AmmoReloadRate * Time.deltaTime;

        // limits ammo to max value
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, MaxAmmo);

        IsCooling = true;
    }
    else
    {
        IsCooling = false;
    }

    if (MaxAmmo == Mathf.Infinity)
    {
        CurrentAmmoRatio = 1f;
    }
    else
    {
        CurrentAmmoRatio = m_CurrentAmmo / MaxAmmo;
    }
}

void UpdateCharge()
{
    if (IsCharging)
    {
        if (CurrentCharge < 1f)
        {
            float chargeLeft = 1f - CurrentCharge;

            // Calculate how much charge ratio to add this frame
            float chargeAdded = 0f;
            if (MaxChargeDuration <= 0f)
            {
                chargeAdded = chargeLeft;
            }
            else
            {
                chargeAdded = (1f / MaxChargeDuration) * Time.deltaTime;
            }

            chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

            // See if we can actually add this charge
            float ammoThisChargeWouldRequire = chargeAdded * AmmoUsageRateWhileCharging;
            if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
            {
                // Use ammo based on charge added
                UseAmmo(ammoThisChargeWouldRequire);

                // set current charge ratio
                CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdded);
            }
        }
    }
}

void UpdateContinuousShootSound()
{
    if (UseContinuousShootSound)
    {
        if (m_WantsToShoot && m_CurrentAmmo >= 1f)
        {
            if (!m_ContinuousShootAudioSource.isPlaying)
            {
                m_ShootAudioSource.PlayOneShot(ShootSfx);
                m_ShootAudioSource.PlayOneShot(ContinuousShootStartSfx);
                m_ContinuousShootAudioSource.Play();
            }
        }
        else if (m_ContinuousShootAudioSource.isPlaying)
        {
            m_ShootAudioSource.PlayOneShot(ContinuousShootEndSfx);
            m_ContinuousShootAudioSource.Stop();
        }
    }
}

public void ShowWeapon(bool show)
{
    WeaponRoot.SetActive(show);

    if (show && ChangeWeaponSfx)
    {
        m_ShootAudioSource.PlayOneShot(ChangeWeaponSfx);
    }

    IsWeaponActive = show;
}

public void UseAmmo(float amount)
{
    m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, MaxAmmo);
    m_CarriedPhysicalBullets -= Mathf.RoundToInt(amount);
    m_CarriedPhysicalBullets = Mathf.Clamp(m_CarriedPhysicalBullets, 0, MaxAmmo);
    m_LastTimeShot = Time.time;
}

public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
{
    m_WantsToShoot = inputDown || inputHeld;
    switch (ShootType)
    {
        case WeaponShootType.Manual:
            if (inputDown)
            {
                return TryShoot();
            }

            return false;

        case WeaponShootType.Automatic:
            if (inputHeld)
            {
                return TryShoot();
            }

            return false;

        case WeaponShootType.Charge:
            if (inputHeld)
            {
                TryBeginCharge();
            }

            // Check if we released charge or if the weapon shoot autmatically when it's fully charged
            if (inputUp || (AutomaticReleaseOnCharged && CurrentCharge >= 1f))
            {
                return TryReleaseCharge();
            }

            return false;

        default:
            return false;
    }
}

bool TryShoot()
{
    if (m_CurrentAmmo >= 1f
        && m_LastTimeShot + DelayBetweenShots < Time.time)
    {
        HandleShoot();
        m_CurrentAmmo -= 1f;

        return true;
    }

    return false;
}

bool TryBeginCharge()
{
    if (!IsCharging
        && m_CurrentAmmo >= AmmoUsedOnStartCharge
        && Mathf.FloorToInt((m_CurrentAmmo - AmmoUsedOnStartCharge) * BulletsPerShot) > 0
        && m_LastTimeShot + DelayBetweenShots < Time.time)
    {
        UseAmmo(AmmoUsedOnStartCharge);

        LastChargeTriggerTimestamp = Time.time;
        IsCharging = true;

        return true;
    }

    return false;
}

bool TryReleaseCharge()
{
    if (IsCharging)
    {
        HandleShoot();

        CurrentCharge = 0f;
        IsCharging = false;

        return true;
    }

    return false;
}

void HandleShoot()
{
    int bulletsPerShotFinal = ShootType == WeaponShootType.Charge
        ? Mathf.CeilToInt(CurrentCharge * BulletsPerShot)
        : BulletsPerShot;

    // spawn all bullets with random direction
    for (int i = 0; i < bulletsPerShotFinal; i++)
    {
        Vector3 shotDirection = GetShotDirectionWithinSpread(WeaponMuzzle);
        ProjectileBase newProjectile = Instantiate(ProjectilePrefab, WeaponMuzzle.position,
            Quaternion.LookRotation(shotDirection));
        newProjectile.Shoot(this);
    }

    // muzzle flash
    if (MuzzleFlashPrefab != null)
    {
        GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, WeaponMuzzle.position,
            WeaponMuzzle.rotation, WeaponMuzzle.transform);
        // Unparent the muzzleFlashInstance
        if (UnparentMuzzleFlash)
        {
            muzzleFlashInstance.transform.SetParent(null);
        }

        Destroy(muzzleFlashInstance, 2f);
    }

    if (HasPhysicalBullets)
    {
        ShootShell();
        m_CarriedPhysicalBullets--;
    }

    m_LastTimeShot = Time.time;

    // play shoot SFX
    if (ShootSfx && !UseContinuousShootSound)
    {
        m_ShootAudioSource.PlayOneShot(ShootSfx);
    }

    // Trigger attack animation if there is any
    if (WeaponAnimator)
    {
        WeaponAnimator.SetTrigger(k_AnimAttackParameter);
    }

    OnShoot?.Invoke();
    OnShootProcessed?.Invoke();
}

public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
{
    float spreadAngleRatio = BulletSpreadAngle / 180f;
    Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere,
        spreadAngleRatio);

    return spreadWorldDirection;
}
    }
}