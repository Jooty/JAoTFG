using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : HumanoidController
{

    [Header("Movement")]
    [SerializeField] private float speed = 0.8f;
    [SerializeField] private float sprintSpeed = 1.3f;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float jumpPower = 5;

    [Header("Maneuver Gear")]
    public float gas;
    public float totalMaxGas;
    public float thrustPower;

    [SerializeField] public GameObject hookUI;
    [SerializeField] public GameObject sword;
    [SerializeField] public GameObject sword2;
    [SerializeField] public ParticleSystem thrustSmoke;
    [SerializeField] public ParticleSystem hookSmoke;

    [HideInInspector] public bool hasSwordInHand;
    [HideInInspector] public bool isThrusting;

    public List<HookController> hooks;
    [SerializeField] private GameObject[] hookPoints;

    [HideInInspector] public AudioClip[] manSoundEffects;
    [HideInInspector] public bool usingManGear;

    private PhysicMaterial zfriction;
    private PhysicMaterial mfriction;

    private Camera cam;

    private Vector3 directionPos;
    private Vector3 moveInput;

    [HideInInspector] public bool isWaitingToLand;
    [HideInInspector] public bool canJump;
    [HideInInspector] public bool isSliding;
    private bool jumpedThisFrame = false;

    private Animator bodyAnim;

    // local components
    [HideInInspector] public Rigidbody rigid;
    private AudioSource aud;
    private Player player;
    private PlayerTargets targets;
    private CapsuleCollider coll;

    private void Awake()
    {
        this.aud = GetComponent<AudioSource>();
        this.rigid = GetComponent<Rigidbody>();
        this.targets = GetComponent<PlayerTargets>();
        this.coll = GetComponent<CapsuleCollider>();
        this.player = GetComponent<Player>();
    }

    private void Start()
    {
        bodyAnim = GetBodyAnimator();
        cam = Camera.main;
        hooks = new List<HookController>();
        //aud.volume = AudioSettings.SFX;

        Cursor.lockState = CursorLockMode.Locked;

        canJump = true;
        gas = totalMaxGas;

        zfriction = Resources.Load<PhysicMaterial>("Physics/zeroFriction");
        mfriction = Resources.Load<PhysicMaterial>("Physics/maxFriction");
        manSoundEffects = Resources.LoadAll<AudioClip>("SFX/HERO");
    }

    private void Update()
    {
        HandleFriction();
        HandleGround();
        HandleGroundedControls();
        HandleAnimations();
        EnforceMaxSpeed();
        ManueverGearUpdate();
    }

    void FixedUpdate()
    {
        if (usingManGear)
        {
            DoManeuverGearPhysics();
        }
        else
        {
            Movement();
        }
    }

    private void HandleGroundedControls()
    {
        // REMOVE LATER
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Break();

        if (Input.GetKeyDown(KeyCode.Space) && canJump && !jumpedThisFrame)
        {
            Jump();
        }
    }

    private void HandleAnimations()
    {
        bodyAnim.SetBool("usingManGear", usingManGear);
        bodyAnim.SetFloat("velocity", Common.GetFloatByRelativePercent(0, 1, 0, GameVariables.HERO_MAX_SPEED, rigid.velocity.magnitude));
        bodyAnim.SetFloat("velocityY", Common.GetFloatByRelativePercent(0, 1, 0, 9.8f, rigid.velocity.y));
        bodyAnim.SetBool("isHooked", hooks.Count > 0);

        if (!usingManGear)
        {
            // Running
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (input != Vector2.zero)
            {
                bodyAnim.SetBool("isRunning", true);
            }
            else
            {
                bodyAnim.SetBool("isRunning", false);
            }
        }
        else
        {
            bodyAnim.SetBool("isRunning", false);
        }

        if (IsGrounded() && usingManGear)
        {
            bodyAnim.SetBool("isSliding", true);
            isSliding = true;
        }
        else
        {
            bodyAnim.SetBool("isSliding", false);
            isSliding = false;
        }
    }

    private Animator GetBodyAnimator()
    {
        return transform.GetChild(0).GetChild(0).GetComponent<Animator>();
    }

    #region Grounded Methods

    private void Movement()
    {
        if (!IsGrounded()) return;

        var _horizontal = Input.GetAxisRaw("Horizontal");
        var _vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(_horizontal, 0, _vertical).normalized;

        RotateToMovement();

        // add camera relativity
        moveInput = cam.transform.TransformDirection(moveInput);
        moveInput.y = 0;
        moveInput.Normalize();

        rigid.AddForce(moveInput * sprintSpeed / Time.deltaTime);
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
                rigid.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);
            }
        }
    }

    private void Jump()
    {
        if (!canJump || !IsGrounded()) return;
        jumpedThisFrame = true;

        bodyAnim.SetTrigger("jump");

        // remove later
        JumpEvent();
    }

    public override void JumpEvent()
    {
        canJump = false;

        rigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }

    private void Land()
    {
        bodyAnim.SetTrigger("land");

        canJump = true;
        isWaitingToLand = false;
        usingManGear = false;
    }

    private void HandleFriction() 
    {
        if (usingManGear) return;

        if (moveInput == Vector3.zero)
        {
            coll.material = mfriction;
        }
        else
        {
            coll.material = zfriction;
        }
    }

    #endregion

    #region Maneuver Gear Methods

    private void ManueverGearUpdate()
    {
        UpdateManeuverGearUI();
        UpdateTetherDistanceWhenFooted();
        CheckHookRunawayDistance();
        HandleManeuverGearControls();
        DrawHook();

        if (!IsGrounded())
        { 
            AirRotate();
        }
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (hasSwordInHand)
            {
                SheatheSwords();
            }
            else
            {
                UnsheatheSwords();
            }
        }

        if (Input.GetMouseButtonDown(0) && hasSwordInHand)
        {
            SwordReady();
        }
        else if (Input.GetMouseButtonUp(0) && hasSwordInHand)
        {
            SwordRelease();
        }

        if (Input.GetKey(KeyCode.Space) && !isSliding)
        {
            if (IsGrounded() && !GetLeftHook() || IsGrounded() && !GetRightHook()) return;

            GasThrust();

            if (!aud.isPlaying)
            {
                aud.clip = manSoundEffects[3];
                aud.loop = true;
                aud.Play();
            }
            thrustSmoke.Play();
        }
        else if (Input.GetKeyUp(KeyCode.Space) || isSliding && !GetLeftHook() || isSliding && !GetRightHook())
        {
            isThrusting = false;
            aud.Stop();
            thrustSmoke.Stop();
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

    private void DrawHook()
    {
        foreach (HookController hook in hooks)
        {
            if (!hook.grapplingLine) continue;

            Vector3[] line_vortex_arr = { hookPoints[(int)hook.side].transform.position, hook.transform.position };
            hook.grapplingLine.SetPositions(line_vortex_arr);
        }
    }

    private void UnsheatheSwords()
    {
        sword.SetActive(true);
        sword2.SetActive(true);

        bodyAnim.SetTrigger("withdrawSword");
        hasSwordInHand = true;

        aud.PlayOneShot(manSoundEffects[0], .2f);
    }

    private void SheatheSwords()
    {
        sword.SetActive(false);
        sword2.SetActive(false);

        bodyAnim.SetTrigger("sheatheSword");
        hasSwordInHand = false;
    }

    private void SwordReady()
    {
        bodyAnim.SetTrigger("attack");
    }

    private void SwordRelease()
    {
        bodyAnim.SetTrigger("attackRelease");
        aud.PlayOneShot(manSoundEffects[5], AudioSettings.SFX);

        // Check for hit
        var distOffset = Vector3.Distance(cam.transform.position, transform.position);
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, distOffset + 3, 1))
        {
            if (hit.transform.tag == "TitanNape")
            {
                hit.transform.gameObject.GetComponent<TitanNape>().Hit();
                aud.PlayOneShot(manSoundEffects[7]);
            }
            else if (hit.transform.tag == "Titan")
            {
                aud.PlayOneShot(manSoundEffects[6]);
            }
        }
        else
        {
            aud.PlayOneShot(manSoundEffects[5]);
        }
    }

    private void AirRotate()
    {
        Quaternion target = Quaternion.identity;

        if (hooks.Count > 0 && rigid.velocity.magnitude > 3)
        {
            var left = GetLeftHook();
            var right = GetRightHook();

            // Solo hook
            if (hooks.Count == 1)
            {
                var hook = GetSoloActiveHook();
                if (hook.status != HookStatus.attached) return;
                if (IsGrounded())
                {
                    target = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
                    return;
                }

                var tetherDirection = hook.transform.position - transform.position;
                target = Quaternion.LookRotation(rigid.velocity, tetherDirection);
            }
            else // Both hooks active
            {
                if (left.status != HookStatus.attached ||right.status != HookStatus.attached) return;

                var lTether = left.transform.position - transform.position;
                var rTether = right.transform.position - transform.position;
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

        if (!isSliding)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * GameVariables.HERO_AIR_ROTATE_SPEED);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * GameVariables.HERO_AIR_ROTATE_SPEED * .135f);
        }
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

            var hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation)
                .GetComponent<HookController>();
            hook.side = side;
            hook.target = hit.point;
            hook.source = this;
            hook.status = HookStatus.released;
            hook.eventualParent = hit.transform;
            hooks.Add(hook);
        }
    }

    private void RecallHook(HookSide side)
    {
        var hook = GetHookBySide(side);
        if (!Common.Exists(hook)) return;
        hook.status = HookStatus.retracting;
        hook.recall = true;

        if (IsGrounded())
        {
            usingManGear = false;
        }

        // sound effect
        aud.PlayOneShot(manSoundEffects[9]);
    }

    public void HookAttachedEvent(HookSide side)
    {
        var hook = GetHookBySide(side);
        if (!hook) return;
        hook.status = HookStatus.attached;
        hook.tetherDistance = Vector3.Distance(hook.transform.position, transform.position);

        transform.LookAt(hook.transform);
        GasBurst();
    }

    public void HookRetractedEvent(HookSide side)
    {
        var hook = GetHookBySide(side);
        hook.status = HookStatus.sheathed;

        hooks.Remove(hook);
        Destroy(hook.gameObject);
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
                if (Vector3.Distance(nextPos, hook.transform.position) < hook.tetherDistance)
                {
                    hook.tetherDistance = Vector3.Distance(nextPos, hook.transform.position);
                }

                if (isThrusting && GameVariables.MG_RETRACT_ON_GAS && hook.tetherDistance > 1 && hooks.Count == 1)
                {
                    hook.tetherDistance -= GameVariables.MG_GAS_REEL_SPEED_MULTIPLIER * Time.deltaTime * GameVariables.MG_GAS_REEL_SPEED_MULTIPLIER;
                }
            }

            ApplyTensionForce(currentVelocity, nextPos);
        }

        return;
    }

    private void ApplyTensionForce(Vector3 currentVelocity, Vector3 nextPos)
    {
        var left = GetLeftHook();
        var right = GetRightHook();

        if (left?.status == HookStatus.attached)
        {
            // Finds what the new velocity is due to tension force grappling hook
            // Normalized vector that from node to test pos
            Vector3 nodeLeft = (nextPos - left.transform.position).normalized;
            Vector3 newPosLeft = (nodeLeft * left.tetherDistance) + left.transform.position;
            Vector3 newVelocityLeft = newPosLeft - transform.position;

            // Force_tension = mass * (d_velo / d_time)
            // Where delta_velocity is new_velocity - old_velocity
            Vector3 deltaVelocityLeft = newVelocityLeft - currentVelocity;
            Vector3 tensionForceLeft = (rigid.mass * (deltaVelocityLeft / Time.fixedDeltaTime));

            rigid.AddForce(tensionForceLeft, ForceMode.Impulse);
        }

        if (right?.status == HookStatus.attached)
        {
            Vector3 nodeRight = (nextPos - right.transform.position).normalized;
            Vector3 newPosRight = (nodeRight * right.tetherDistance) + right.transform.position;
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

    private void HandleGround()
    {
        if (IsGrounded())
        { 
            if (usingManGear)
            {
                if (isWaitingToLand && rigid.velocity.magnitude < 3)
                {
                    Land();
                }
                else if (isWaitingToLand && rigid.velocity.magnitude > 3 && hooks.Count == 0)
                {
                    rigid.drag = .5f;
                }
            }
            else
            {
                if (isWaitingToLand && !jumpedThisFrame)
                {
                    Land();
                }
                else
                {
                    rigid.drag = 5;
                }
            }
        }
        else
        {
            rigid.drag = 0.02f;

            isWaitingToLand = true;
            jumpedThisFrame = false;

            if (hooks.Count > 0)
            {
                usingManGear = true;
            }
        }
    }

    public bool IsGrounded()
    {
        var origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        if (GameVariables.DEBUG_DRAW_GROUND_CHECK_RAY)
        {
            Debug.DrawLine(origin, (Vector3.down * (coll.bounds.extents.y + .1f)) + origin, Color.red);
        }

        return Physics.Raycast(origin, Vector3.down, coll.bounds.extents.y + .1f, 1);
    }

    private void EnforceMaxSpeed()
    {
        if (rigid.velocity.magnitude > GameVariables.HERO_MAX_SPEED)
        {
            rigid.velocity = rigid.velocity.normalized * GameVariables.HERO_MAX_SPEED;
        }
    }

    private Vector3 GetNextFramePosition()
    {
        return transform.position + (rigid.velocity * Time.fixedDeltaTime);
    }

    public override void ColliderEvent(Collision coll)
    {
        return;
    }
}