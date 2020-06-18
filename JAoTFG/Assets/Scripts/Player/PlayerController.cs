using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : CharacterController
{

    [Header("Maneuver Gear")]
    public bool hasManGear;
    public float gas;
    public float totalMaxGas;
    public float thrustPower;

    [SerializeField] public GameObject hookUI;
    [SerializeField] public ParticleSystem thrustSmoke;
    [SerializeField] public ParticleSystem hookSmoke;

    [HideInInspector] public bool isThrusting;

    public List<HookController> hooks;
    public Transform[] ropeShotVisualizerSpawnPoints_Left;
    public Transform[] ropeShotVisualizerSpawnPoints_Right;
    public GameObject[] hookPoints;

    [HideInInspector] public AudioClip[] manSoundEffects;
    [HideInInspector] public bool usingManGear;

    private PhysicMaterial zfriction;
    private PhysicMaterial mfriction;

    private Camera cam;

    private Vector3 directionPos;
    private Vector3 moveInput;

    [HideInInspector] public bool isSliding;

    // local components
    private AudioSource aud;

    new private void Awake()
    {
        this.aud = GetComponent<AudioSource>();

        base.Awake();
    }

    private void Start()
    { 
        cam = Camera.main;
        hooks = new List<HookController>();
        // TODO
        //aud.volume = AudioSettings.SFX;

        Cursor.lockState = CursorLockMode.Locked;

        canJump = true;
        gas = totalMaxGas;

        zfriction = Resources.Load<PhysicMaterial>("Physics/zeroFriction");
        mfriction = Resources.Load<PhysicMaterial>("Physics/maxFriction");
        manSoundEffects = Resources.LoadAll<AudioClip>("SFX/HERO");
    }

    new private void Update()
    {
        HandleGround();
        HandleGroundedControls();
        ManueverGearUpdate();
        SetSliding();

        if (!IsGrounded())
        {
            AirRotate();
        }
        else if (base.IsGrounded() && base.currentSpeed > 15)
        {
            DoSliding();
        }

        base.Update();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void HandleGroundedControls()
    {
        // REMOVE LATER
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Break();

        if (Input.GetKeyDown(KeyCode.Space) && canJump || Input.GetKeyDown(KeyCode.Space) && canDoubleJump)
        {
            if (!jumpedThisFrame && canJump)
            {
                base.Jump();
            }
            else if (canDoubleJump)
            {
                base.Jump();
            }
        }
    }

    public override void Move()
    {
        if (IsGrounded() && hooks.Count == 0 && !isWaitingToLand)
        {
            var _horizontal = Input.GetAxisRaw("Horizontal");
            var _vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector3(_horizontal, 0, _vertical).normalized;

            RotateToMovement();
            HandleFriction();

            // add camera relativity
            moveInput = cam.transform.TransformDirection(moveInput);
            moveInput.y = 0;
            moveInput.Normalize();

            base.rigid.AddForce(moveInput * sprintSpeed / Time.deltaTime);
        }
        else if (usingManGear && hasManGear)
        {
            DoManeuverGearPhysics();
        }

        base.Move();
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
            base.rigid.drag = 0.02f;
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
            || IsGrounded() && hooks.Count > 0 && rigid.velocity.magnitude > 3f && usingManGear)
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }
    }

    private void DoSliding()
    {
        if (!isSliding) return;

        if (hooks.Count == 0)
        {
            RotateToMovement();
        }
        else
        {
            var hook = GetSoloActiveHook();
            if (hook.status != HookStatus.attached) return;

            var tetherDirection = hook.transform.position - transform.position;
            var lookDir = Quaternion.LookRotation(rigid.velocity, tetherDirection);
            var newDir = new Quaternion(0, lookDir.y, 0, 0);
            var target = newDir;

            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * GameVariables.HERO_AIR_ROTATE_SPEED * .15f);
        }
    }

    private void RotateToMovement()
    {
        var directionPos = transform.position + (cam.transform.right * moveInput.x) + (cam.transform.forward * moveInput.z);
        var dir = directionPos - transform.position;
        dir.y = 0;

        // rotate
        if (moveInput.x != 0 || moveInput.z != 0)
        {
            var angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir));

            if (angle != 0)
            {
                base.rigid.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);
            }
        }
    }

    private void HandleFriction()
    {
        if (usingManGear) return;

        if (moveInput == Vector3.zero)
        {
            base.Collider.material = mfriction;
        }
        else
        {
            base.Collider.material = zfriction;
        }
    }

    #region Maneuver Gear Methods

    private void ManueverGearUpdate()
    {
        if (!hasManGear) return; 

        UpdateManeuverGearUI();
        UpdateTetherDistanceWhenFooted();
        CheckHookRunawayDistance();
        HandleManeuverGearControls();
    }

    private void HandleManeuverGearControls()
    {
        if (Input.GetKey(KeyCode.Q) && CanHook() && !GetLeftHook())
        {
            FireHook(HookSide.left);
        }
        else if (Input.GetKeyUp(KeyCode.Q) && GetLeftHook())
        {
            RecallHook(HookSide.left);
        }

        if (Input.GetKey(KeyCode.E) && CanHook() && !GetRightHook())
        {
            FireHook(HookSide.right);
        }
        else if (Input.GetKeyUp(KeyCode.E) && GetRightHook())
        {
            RecallHook(HookSide.right);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            GasThrust();

            if (!aud.isPlaying)
            {
                aud.clip = manSoundEffects[3];
                aud.loop = true;
                aud.Play();
            }
            if (thrustSmoke)
            {
                thrustSmoke.Play();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) || isSliding && !GetLeftHook() || isSliding && !GetRightHook())
        {
            isThrusting = false;
            aud.Stop();
            if (thrustSmoke)
            {
                thrustSmoke.Stop();
            }
        }
    }

    private void UpdateManeuverGearUI()
    {
        if (!hookUI) return;

        if (CanHook())
        {
            hookUI.SetActive(true);
        }
        else
        {
            hookUI.SetActive(false);
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
            if (Vector3.Distance(transform.position, hook.transform.position) > GameVariables.MG_HOOK_MAX_RUNAWAY_RANGE)
            {
                RecallHook(hook.side);
            }
        }
    }

    private void GasThrust()
    {
        isThrusting = true;
        gas -= 1 * Time.deltaTime;

        rigid.AddForce(transform.forward * thrustPower * Time.deltaTime);
    }

    private void GasBurst()
    {
        gas -= 1.5f * Time.deltaTime;

        rigid.AddForce(transform.forward * thrustPower * .75f, ForceMode.Impulse);
    }

    private void AirRotate()
    {
        Quaternion target = Quaternion.identity;

        if (hooks.Count > 0 && rigid.velocity.magnitude > 3)
        {
            // Solo hook
            if (hooks.Count == 1)
            {
                var hook = GetSoloActiveHook();
                if (hook.status != HookStatus.attached) return;
                if (!IsGrounded())
                {
                    var tetherDirection = hook.getLastPoint() - transform.position;
                    target = Quaternion.LookRotation(rigid.velocity, tetherDirection);
                }
            }
            else // Both hooks active
            {
                var left = GetLeftHook();
                var right = GetRightHook();

                if (left.status != HookStatus.attached || right.status != HookStatus.attached) return;

                var lTether = left.getLastPoint() - transform.position;
                var rTether = right.getLastPoint() - transform.position;
                var lRot = Quaternion.LookRotation(rigid.velocity, lTether);
                var rRot = Quaternion.LookRotation(rigid.velocity, rTether);

                target = Quaternion.Lerp(lRot, rRot, .5f);
            }
        }
        else // Rotate WASD
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

        transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * GameVariables.HERO_AIR_ROTATE_SPEED);
    }

    private bool CanHook()
    {
        return Physics.Raycast(cam.transform.position, cam.transform.forward, GameVariables.MG_HOOK_RANGE, 1);
    }

    private void FireHook(HookSide side)
    {
        // SFX
        // .hookSmoke.Play();
        aud.PlayOneShot(manSoundEffects[10]);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, GameVariables.MG_HOOK_RANGE, 1))
        {
            if (rigid.velocity.y < -5)
            {
                usingManGear = true;
            }

            // New code
            if (side == HookSide.left)
            {
                var hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation)
                    .GetComponent<HookController>();
                hook.transform.parent = transform;
                hook.transform.position = hookPoints[0].transform.position;
                hook.InitateHook(HookSide.left, this, hookPoints[0].transform, hit.point, hit.transform.gameObject, ropeShotVisualizerSpawnPoints_Left);
                hooks.Add(hook);

                hook.OnHookRecalled += Hook_OnHookRecalled;
            } 
            else
            {
                var hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation)
                    .GetComponent<HookController>();
                hook.transform.position = hookPoints[1].transform.position;
                hook.transform.position = hookPoints[1].transform.position;
                hook.InitateHook(HookSide.right, this, hookPoints[1].transform, hit.point, hit.transform.gameObject, ropeShotVisualizerSpawnPoints_Right);
                hooks.Add(hook);

                hook.OnHookRecalled += Hook_OnHookRecalled;
            }
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

        // sound effect
        aud.PlayOneShot(manSoundEffects[9]);
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

                if (isThrusting && GameVariables.MG_RETRACT_ON_GAS && hook.tetherDistance > 1 && hooks.Count == 1)
                {
                    hook.tetherDistance -= GameVariables.MG_GAS_REEL_SPEED_MULTIPLIER * Time.deltaTime;
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

    #endregion

    private Vector3 GetNextFramePosition()
    {
        return transform.position + (rigid.velocity * Time.fixedDeltaTime);
    }

    public override void ColliderEvent(Collision coll)
    {
        return;
    }
}