using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : CharacterController
{
    [Header("Maneuver Gear")]
    public bool hasManGear;
    public float thrustPower;
    public float maxGas;
    [HideInInspector] public float currentGas;

    [HideInInspector] public bool isThrusting;
    private List<HookController> hooks;
    public Transform[] ropeShotVisualizerSpawnPoints_Left;
    public Transform[] ropeShotVisualizerSpawnPoints_Right;
    public GameObject[] hookPoints;

    private bool isHoldingAttack;
    private float attackHoldTime;

    [HideInInspector] public AudioClip[] manSoundEffects;
    [HideInInspector] public bool usingManGear;

    private Vector3 directionPos;

    [HideInInspector] public bool isSliding;

    [Header("Debug")]
    public bool drawSwordHitbox = false;

    // local components
    private AudioSource aud;

    new private void Awake()
    {
        this.aud = GetComponent<AudioSource>();

        currentGas = maxGas;

        base.Awake();
    }

    private void Start()
    {
        hooks = new List<HookController>();
        // TODO
        //aud.volume = AudioSettings.SFX;

        Cursor.lockState = CursorLockMode.Locked;
        manSoundEffects = Resources.LoadAll<AudioClip>("SFX/HERO");
    }

    new private void Update()
    {
        base.Update();

        HandleGround();
        ManueverGearUpdate();
        SetSliding();

        if (!base.IsGrounded())
        {
            AirRotate();
        }
    }

    public override void Land()
    {
        usingManGear = false;

        base.Land();
    }

    public override void HandleGround()
    {
        if (base.IsGrounded() && base.isWaitingToLand)
        {
            if (usingManGear)
            {
                if (base.currentSpeed < 15)
                {
                    Land();
                }
                else if (base.currentSpeed > 15 && hooks.Count == 0) // sliding on ground
                {
                    base.rigid.drag = .4f;
                }
            }
            else
            {
                if (!base.jumpedThisFrame && base.currentSpeed < 15f)
                {
                    Land();
                }
                else if (isThrusting)
                {
                    base.rigid.drag = .4f;
                }
                else
                {
                    base.rigid.drag = 5;
                }
            }
        }
        else if (!base.IsGrounded())
        {
            base.rigid.drag = 0.07f;
            base.isWaitingToLand = true;
            base.jumpedThisFrame = false;

            if (hooks.Count > 0)
            {
                usingManGear = true;
            }
        }
    }

    private void SetSliding()
    {
        if (IsGrounded() && rigid.velocity.magnitude > 15f && !base.jumpedThisFrame
            || IsGrounded() && hooks.Count > 0 && rigid.velocity.magnitude > 3f)
        {
            isSliding = true;
            DoSlidingControl();

            base.characterBody.PlaySFXParticles(CharacterSFXType.sliding_leaves);
        }
        else
        {
            isSliding = false;
            base.characterBody.StopSFXParticles(CharacterSFXType.sliding_leaves);
        }
    }

    private void DoSlidingControl()
    {
        if (!isSliding) return;

        if (hooks.Count == 0)
        {
            base.RotateToMovement();
        }
        else
        {
            var hook = GetSoloActiveHook();
            if (hook.status != HookStatus.attached) return;

            var tetherDirection = hook.transform.position - transform.position;
            var lookDir = Quaternion.LookRotation(rigid.velocity, tetherDirection);
            var newDir = new Quaternion(0, lookDir.y, 0, 0);
            var target = newDir;

            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * Gamerules.HERO_AIR_ROTATE_SPEED * .15f);
        }
    }

    #region Ability Overrides

    protected override void Ability_01_Press()
    {
        base.Ability_01_Press();

        if (IsPlayerInHookRange() && !GetLeftHook())
        {
            FireHook(HookSide.left);
        }
    }

    protected override void Ability_01_Release()
    {
        base.Ability_01_Release();

        if (GetLeftHook())
        {
            RecallHook(HookSide.left);
        }
    }

    protected override void Ability_02_Press()
    {
        base.Ability_02_Press();

        if (IsPlayerInHookRange() && !GetRightHook())
        {
            FireHook(HookSide.right);
        }
    }

    protected override void Ability_02_Release()
    {
        base.Ability_02_Release();

        if (GetRightHook())
        {
            RecallHook(HookSide.right);
        }
    }

    protected override void Ability_03_Press()
    {
        base.Ability_03_Press();

        RefillGas();
    }

    public override void Jump()
    {
        if (hasManGear && currentGas > 0)
        {
            base.characterBody.PlaySFXAudio(CharacterSFXType.gas_thrust, manSoundEffects[3]);
            base.characterBody.PlaySFXParticles(CharacterSFXType.gas_thrust);
        }

        if (base.IsGrounded())
        {
            base.Jump();
        }
    }

    public override void JumpHold()
    {
        base.JumpHold();

        if (hasManGear && currentGas > 0)
        {
            GasThrust();
        }
        else if (isThrusting && currentGas <= 0)
        {
            base.characterBody.StopAllSFX(CharacterSFXType.gas_thrust);
        }
    }

    public override void JumpRelease()
    {
        base.JumpRelease();

        if (isThrusting)
        {
            isThrusting = false;

            base.characterBody.StopAllSFX(CharacterSFXType.gas_thrust);
        }
    }

    #endregion

    #region Maneuver Gear Methods

    private void ManueverGearUpdate()
    {
        if (!hasManGear) return;

        UpdateTetherDistanceWhenFooted();
        CheckHookRunawayDistance();
        DoSwordTrail();

        if (usingManGear)
        {
            DoManeuverGearPhysics();
        }

        if (isHoldingAttack)
        {
            attackHoldTime += Time.deltaTime;
        }
    }

    private void UpdateTetherDistanceWhenFooted()
    {
        if (usingManGear) return;

        foreach (HookController hook in hooks)
        {
            hook.tetherDistance = Vector3.Distance(transform.position, hook.transform.position);
        }
    }

    private void CheckHookRunawayDistance()
    {
        if (hooks.Count == 0) return;

        foreach (HookController hook in hooks)
        {
            if (Vector3.Distance(transform.position, hook.transform.position) > Gamerules.MG_HOOK_RANGE * 1.5f)
            {
                RecallHook(hook.side);
            }
        }
    }

    private void GasThrust()
    {
        isThrusting = true;

        currentGas -= Time.deltaTime * 30f;

        rigid.AddForce(transform.forward * thrustPower * Time.deltaTime);
    }

    private void AirRotate()
    {
        Quaternion target = Quaternion.identity;
        var left = GetLeftHook();
        var right = GetRightHook();
        bool eitherAttached = left?.status == HookStatus.attached || right?.status == HookStatus.attached;

        if (eitherAttached && base.currentSpeed > 3)
        {
            if (hooks.Count == 1) // Solo hook
            {
                var hook = GetSoloActiveHook();
                if (!IsGrounded())
                {
                    var tetherDirection = hook.getLastPoint() - transform.position;
                    target = Quaternion.LookRotation(rigid.velocity, tetherDirection);
                }
            }
            else // Both hooks active
            {
                var lTether = left.getLastPoint() - transform.position;
                var rTether = right.getLastPoint() - transform.position;
                var lRot = Quaternion.LookRotation(rigid.velocity, lTether);
                var rRot = Quaternion.LookRotation(rigid.velocity, rTether);

                target = Quaternion.Lerp(lRot, rRot, .5f);
            }
        }
        else if (isHoldingAttack)
        {
            Quaternion quat = Quaternion.identity;
            quat.eulerAngles = new Vector3(transform.localEulerAngles.x, cam.transform.localEulerAngles.y, transform.localEulerAngles.z);
            target = quat;
        }
        else // rotate WASD
        {
            var _horizontal = Input.GetAxisRaw("Horizontal");
            var _vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector3(_horizontal, 0, _vertical).normalized;

            directionPos = transform.position + (cam.transform.right * moveInput.x) + (cam.transform.forward * moveInput.z);
            var dir = directionPos - transform.position;
            dir.y = 0;

            // rotate
            if (moveInput.x != 0 || moveInput.z != 0)
            {
                var angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));

                if (angle != 0)
                {
                    target = Quaternion.LookRotation(dir);
                }
            }
            else
            {
                target = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * Gamerules.HERO_AIR_ROTATE_SPEED);
    }

    public bool IsPlayerInHookRange()
    {
        return Physics.Raycast(cam.transform.position, cam.transform.forward, Gamerules.MG_HOOK_RANGE, 1);
    }

    private void FireHook(HookSide side)
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, Gamerules.MG_HOOK_RANGE, 1))
        {
            if (rigid.velocity.y < -5)
            {
                usingManGear = true;
            }

            var hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation)
                    .GetComponent<HookController>();
            hook.transform.position = hookPoints[(int)side].transform.position;
            hook.transform.position = hookPoints[(int)side].transform.position;
            hook.InitateHook(side, this, hookPoints[(int)side].transform, hit.point, hit.transform.gameObject, ropeShotVisualizerSpawnPoints_Right);
            hooks.Add(hook);

            hook.OnHookRecalled += Hook_OnHookRecalled;

            // play SFX
            CharacterSFXType type = (side == HookSide.left) ? CharacterSFXType.gas_hook_left : CharacterSFXType.gas_hook_right;

            base.characterBody.PlaySFXAudioOneShot(type, manSoundEffects[10]);
            base.characterBody.PlaySFXParticles(type);
        }
    }

    private void RecallHook(HookSide side)
    {
        var hook = GetHookBySide(side);
        if (!hook || hook.recall) return;

        hook.recall = true;

        if (IsGrounded())
        {
            usingManGear = false;
        }

        // sfx
        CharacterSFXType type = (side == HookSide.left) ? CharacterSFXType.gas_hook_left : CharacterSFXType.gas_hook_right;
        base.characterBody.PlaySFXAudioOneShot(type, manSoundEffects[9]);
    }

    private void Hook_OnHookRecalled(object sender, EventArgs e)
    {
        hooks.Remove((HookController)sender);
    }

    private void DoManeuverGearPhysics()
    {
        var left = GetLeftHook();
        var right = GetRightHook();

        if (left?.status == HookStatus.attached || right?.status == HookStatus.attached)
        {
            // gets velocity in units/frame, then gets the position for next frame
            Vector3 currentVelocity = rigid.velocity * Time.fixedDeltaTime;
            Vector3 nextPos = transform.position + currentVelocity;

            foreach (HookController hook in hooks)
            {
                if (Vector3.Distance(nextPos, hook.target) < hook.tetherDistance)
                {
                    hook.tetherDistance = Vector3.Distance(nextPos, hook.target);
                }

                if (isThrusting && Gamerules.MG_RETRACT_ON_GAS && hook.tetherDistance > 1 && hooks.Count == 1)
                {
                    hook.tetherDistance -= Gamerules.MG_GAS_REEL_SPEED_MULTIPLIER * Time.deltaTime;
                }
            }

            ApplyTensionForce(currentVelocity, nextPos);
        }
    }

    private void ApplyTensionForce(Vector3 currentVelocity, Vector3 nextPos)
    {
        var left = GetLeftHook();
        var right = GetRightHook();

        if (left?.status == HookStatus.attached)
        {
            // Finds what the new velocity is due to tension force grappling hook
            // Normalized vector that from node to test pos
            Vector3 nodeLeft = (nextPos - left.target).normalized;
            Vector3 newPosLeft = (nodeLeft * left.tetherDistance) + left.target;
            Vector3 newVelocityLeft = newPosLeft - transform.position;

            // Force_tension = mass * (d_velo / d_time)
            // Where delta_velocity is new_velocity - old_velocity
            Vector3 deltaVelocityLeft = newVelocityLeft - currentVelocity;
            Vector3 tensionForceLeft = (rigid.mass * (deltaVelocityLeft / Time.fixedDeltaTime));

            rigid.AddForce(tensionForceLeft, ForceMode.Impulse);
        }

        if (right?.status == HookStatus.attached)
        {
            Vector3 nodeRight = (nextPos - right.target).normalized;
            Vector3 newPosRight = (nodeRight * right.tetherDistance) + right.target;
            Vector3 newVelocityRight = newPosRight - transform.position;

            Vector3 deltaVelocityRight = newVelocityRight - currentVelocity;
            Vector3 tensionForceRight = (rigid.mass * (deltaVelocityRight / Time.fixedDeltaTime));

            rigid.AddForce(tensionForceRight, ForceMode.Impulse);
        }
    }

    private void DoSwordTrail()
    {
        // if moving 10% near max speed, do trail
        float percentVal = 0.1f * Gamerules.HERO_MAX_SPEED;
        float needAboveThis = Gamerules.HERO_MAX_SPEED - percentVal;
        if (base.currentSpeed >= needAboveThis)
        {
            base.characterBody.PlaySFXTrails(CharacterSFXType.sword_trail);
        }
        else
        {
            base.characterBody.StopSFXTrails(CharacterSFXType.sword_trail);
        }
    }

    public override void AttackRelease()
    {
        base.AttackRelease();

        isHoldingAttack = false;
        attackHoldTime = 0;

        var pos = getSwordHitboxLocation();
        Vector3 hitboxSizeHalf = new Vector3(1.25f, 0.375f, 0.5f);

        Collider[] hitColliders = Physics.OverlapBox(pos, hitboxSizeHalf, base.characterBody.transform.rotation);
        if (hitColliders.Length > 0)
        {
            Attack_Hit(hitColliders);
        }
        else
        {
            aud.PlayOneShot(manSoundEffects[5]);
        }
    }

    protected override void Attack_Hit(Collider[] colliders)
    {
        // TODO: revise
        if (colliders.Any(x => x.tag.Contains("Titan")))
        {
            // sfx
            aud.PlayOneShot(manSoundEffects[6]);

            GameObject firstTitanPartHit = colliders.FirstOrDefault(x => x.tag.Contains("Titan")).gameObject;

            // LOS check
            var direction = transform.position - firstTitanPartHit.transform.position;
            if (Physics.Raycast(transform.position, direction, out var hit, 0))
            {
                if (hit.transform != firstTitanPartHit.transform)
                {
                    return;
                }
            }

            if (firstTitanPartHit.CompareTag("TitanHitbox"))
            {
                var info = new DeathInfo(this, (int)base.currentSpeed, DetermineScore());
                firstTitanPartHit.GetComponent<TitanBodyHitbox>().Hit(info);
            }
        }
    }

    public void RefillGas()
    {
        currentGas = maxGas;
    }

    public int DetermineScore()
    {
        var holdTime = Mathf.Clamp(attackHoldTime, 0, 5);
        var scaledHoldTime = Common.GetFloatByRelativePercent(10, 5000, 0, 5, holdTime);
        var speedScore = Common.GetFloatByRelativePercent(10, 5000, 0, Gamerules.HERO_MAX_SPEED, base.currentSpeed);

        return (int)(scaledHoldTime + speedScore);
    }

    public HookController GetLeftHook()
    {
        return hooks.FirstOrDefault(x => x.side == HookSide.left);
    }

    public HookController GetRightHook()
    {
        return hooks.FirstOrDefault(x => x.side == HookSide.right);
    }

    private HookController GetSoloActiveHook()
    {
        if (hooks.Count > 0)
        {
            return hooks.FirstOrDefault();
        }
        else
        {
            return null;
        }
    }

    private HookController GetHookBySide(HookSide side)
    {
        return hooks.FirstOrDefault(x => x.side == side);
    }

    private Vector3 getSwordHitboxLocation()
    {
        var t = characterBody.transform;
        Vector3 hitboxPosition = t.position + t.forward;
        return hitboxPosition.ChangeY(t.position.y + 1);
    }

    public bool isHooked()
    {
        return hooks.Count > 0;
    }

    #endregion Maneuver Gear Methods

    private void OnDrawGizmos()
    {
        if (!characterBody || !drawSwordHitbox) return;

        Gizmos.color = Color.white;
        Gizmos.matrix = characterBody.transform.localToWorldMatrix;

        var hitboxPosition = getSwordHitboxLocation();
        Vector3 hitboxSize = new Vector3(1.25f * 2f, 0.375f * 2f, 0.5f * 2f);
        hitboxPosition = characterBody.transform.InverseTransformPoint(hitboxPosition);

        Gizmos.DrawCube(hitboxPosition, hitboxSize);
    }
}