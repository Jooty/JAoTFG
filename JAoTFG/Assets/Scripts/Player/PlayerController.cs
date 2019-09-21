using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float speed = 0.8f;
    [SerializeField] private float sprintSpeed = 1.3f;
    [SerializeField] private float turnSpeed = 5;
    [SerializeField] private float jumpPower = 5;

    private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition, rightFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    [SerializeField] private Transform leftFootTransform, rightFootTransform;
    [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] [SerializeField] private float feetToIkPositionSpeed = 0.5f;
    public bool showSolverDebug = true;

    [Header("Maneuver Gear")]
    public float gas;
    public float totalMaxGas;
    public float thrustPower;
    public float hookSpeed;
    [HideInInspector] public float hookReturnSpeed;

    [SerializeField] public GameObject hookUI;
    [SerializeField] public GameObject sword;
    [SerializeField] public GameObject sword2;
    [SerializeField] public ParticleSystem thrustSmoke;
    [SerializeField] public ParticleSystem hookSmoke;
    [SerializeField] public Transform leftHookPoint;

    [HideInInspector] public bool hasSwordInHand;
    [HideInInspector] public bool isThrusting;

    [HideInInspector] public HookController hook;
    [HideInInspector] public GameObject grapplingLine;
    [HideInInspector] public HookStatus hookStatus;
    [HideInInspector] public float hookDistance;

    [HideInInspector] public AudioClip[] manSoundEffects;
    private bool usingManGear;

    private PhysicMaterial zfriction;
    private PhysicMaterial mfriction;

    private Camera cam;

    private Vector3 directionPos;
    private Vector3 moveInput;

    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isWaitingToLand;
    [HideInInspector] public bool canJump;
    [HideInInspector] public bool wantsToJump;
    [HideInInspector] public bool isSliding;

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
        //aud.volume = AudioSettings.SFX;

        Cursor.lockState = CursorLockMode.Locked;

        zfriction = Resources.Load<PhysicMaterial>("Physics/zeroFriction");
        mfriction = Resources.Load<PhysicMaterial>("Physics/maxFriction");

        canJump = true;
        wantsToJump = true;
        gas = totalMaxGas;
        hookReturnSpeed = hookSpeed * 3f;
        hookStatus = HookStatus.sheathed;

        CreateTetherLine();
    }

    private void Update()
    {
        HandleFriction();
        HandleGround();
        HandleControls();
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
            FootIkFixedUpdate();
        }
    }

    private void HandleControls()
    {
        // REMOVE LATER
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Break();

        if (Input.GetKey(KeyCode.Space) && canJump)
        {
            if (wantsToJump)
            {
                wantsToJump = false;
                Jump();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            wantsToJump = true;
        }
    }

    private void HandleAnimations()
    {
        bodyAnim.SetBool("usingManGear", usingManGear);
        bodyAnim.SetFloat("velocity", Common.GetFloatByRelativePercent(0, 1, 0, GameVariables.HERO_MAX_SPEED, rigid.velocity.magnitude));
        bodyAnim.SetFloat("velocityY", Common.GetFloatByRelativePercent(0, 1, 0, 9.8f, rigid.velocity.y));
        bodyAnim.SetBool("isHooked", hookStatus == HookStatus.attached);

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

        if (IsGrounded && usingManGear)
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
        if (!IsGrounded) return;

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
        if (!canJump || !IsGrounded) return;

        bodyAnim.SetTrigger("jump");
    }

    public void JumpEvent()
    {
        canJump = false;

        if (hookStatus != HookStatus.sheathed)
        {
            usingManGear = true;
            rigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
        else
        {
            rigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
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

    #region Foot Grounding

    private void FootIkFixedUpdate()
    {
        if (!enableFeetIk) return;

        AdjustFeetTargets();

        FootPositonSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation);
        FootPositonSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!enableFeetIk) return;

        MovePelvisHeight();

        // right foot
        bodyAnim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        MoveFootToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

        // left foot
        bodyAnim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        MoveFootToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
    }

    private void MoveFootToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = bodyAnim.GetIKPosition(foot);

        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float y = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += y;

            lastFootPositionY = y;

            targetIkPosition = transform.TransformPoint(targetIkPosition);

            bodyAnim.SetIKRotation(foot, rotationIkHolder);
        }

        bodyAnim.SetIKPosition(foot, targetIkPosition);
    }

    private void MovePelvisHeight()
    {
        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = bodyAnim.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = bodyAnim.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        bodyAnim.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = bodyAnim.bodyPosition.y;
    }

    private void FootPositonSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast));

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out var hit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = hit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

            return;
        }

        feetIkPositions = Vector3.zero;
    }

    private void AdjustFeetTargets ()
    {
        rightFootPosition = rightFootTransform.position;
        rightFootPosition.y = transform.position.y + heightFromGroundRaycast;

        leftFootPosition = leftFootTransform.position;
        leftFootPosition.y = transform.position.y + heightFromGroundRaycast;
    }

    #endregion

    #region Maneuver Gear Methods

    private void ManueverGearUpdate()
    {
        UpdateManeuverGearCameraEffects();
        UpdateManeuverGearUI();
        CheckHookRunawayDistance();
        HandleManeuverGearControls();
        DrawHook();

        if (!usingManGear && hook) 
        {
            // todo
            hookDistance = Vector3.Distance(transform.position, hook.transform.position);
        }

        if (usingManGear)
        { 
            AirRotate();
        }
    }

    private void HandleManeuverGearControls()
    {
        if (Input.GetKey(KeyCode.Q) && !hook && CanHook())
        {
            FireHook();
        }
        else if (Input.GetKeyUp(KeyCode.Q) && hook)
        {
            RecallHook();
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

        if (Input.GetKey(KeyCode.Space))
        {
            if (IsGrounded && !hook) return;

            GasThrust();

            if (!aud.isPlaying)
            {
                aud.clip = manSoundEffects[3];
                aud.loop = true;
                aud.Play();
            }
            thrustSmoke.Play();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isThrusting = false;
            aud.Stop();
            thrustSmoke.Stop();
        }
    }

    private void UpdateManeuverGearUI()
    {
        if (CanHook())
        {
            hookUI.SetActive(true);
        }
        else
        {
            hookUI.SetActive(false);
        }
    }

    private void UpdateManeuverGearCameraEffects()
    {
        if (!usingManGear) return;

        var target = Common.GetFloatByRelativePercent(GameVariables.FIELD_OF_VIEW, GameVariables.FIELD_OF_VIEW * 1.2f, 0, GameVariables.HERO_MAX_SPEED, rigid.velocity.magnitude);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, .3f);
    }

    private void CheckHookRunawayDistance()
    {
        if (!hook) return;

        if (Vector3.Distance(transform.position, hook.transform.position) > GameVariables.MG_HOOK_MAX_RUNAWAY_RANGE)
        {
            RecallHook();
        }
    }

    private void GasThrust()
    {
        rigid.AddForce(transform.forward * thrustPower * Time.deltaTime, ForceMode.Acceleration);
        isThrusting = true;

        gas -= 1 * Time.deltaTime;
    }

    private void CreateTetherLine()
    {
        // draw hook
        grapplingLine = new GameObject("GrapplingLine");
        grapplingLine.SetActive(false);
        grapplingLine.transform.SetParent(leftHookPoint);
        LineRenderer line_renderer = grapplingLine.AddComponent<LineRenderer>();
        line_renderer.startWidth = .05f;
        line_renderer.material.color = Color.black;

        manSoundEffects = Resources.LoadAll<AudioClip>("SFX/HERO");
    }

    private void DrawHook()
    {
        if (!hook) return;

        Vector3[] line_vortex_arr = { leftHookPoint.position, hook.transform.position };
        grapplingLine.GetComponent<LineRenderer>().SetPositions(line_vortex_arr);
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

        if (hookStatus == HookStatus.attached && rigid.velocity.magnitude > 3 && hook)
        {
            if (IsGrounded)
            {
                target = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);

                return;
            }

            var tetherDirection = hook.transform.position - transform.position;
            target = Quaternion.LookRotation(rigid.velocity, tetherDirection);
        }
        else
        {
            if (isSliding) return;

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

    private void FireHook()
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

            var _hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation);
            hook = _hook.GetComponent<HookController>();
            hook.target = hit.point;
            hook.source = this;
            hookStatus = HookStatus.released;

            grapplingLine.SetActive(true);
        }
    }

    private void RecallHook()
    {
        hookStatus = HookStatus.retracting;

        // sound effect
        aud.PlayOneShot(manSoundEffects[9]);

        hook.recall = true;

        if (IsGrounded)
        {
            usingManGear = false;
        }

        return;
    }

    public void HookAttachedEvent()
    {
        hookStatus = HookStatus.attached;

        hookDistance = Vector3.Distance(hook.transform.position, gameObject.transform.position);
    }

    private void DoManeuverGearPhysics()
    {
        if (Input.GetKey(KeyCode.Q) && hookStatus == HookStatus.attached && hook)
        {
            // gets velocity in units/frame, then gets the position for next frame
            Vector3 currentVelocityUpf = rigid.velocity * Time.fixedDeltaTime;
            Vector3 nextPos = transform.position + currentVelocityUpf;

            if (Vector3.Distance(nextPos, hook.transform.position) < hookDistance)
            {
                hookDistance = Vector3.Distance(nextPos, hook.transform.position);
            }
            if (isThrusting && GameVariables.MG_RETRACT_ON_GAS)
            {
                hookDistance -= GameVariables.MG_GAS_REEL_SPEED_MULTIPLIER * Time.deltaTime * GameVariables.MG_GAS_REEL_SPEED_MULTIPLIER;
            }

            ApplyTensionForce(currentVelocityUpf, nextPos);
        }

        return;
    }

    private void ApplyTensionForce(Vector3 currentVelocityUpf, Vector3 nextPos)
    {
        //finds what the new velocity is due to tension force grappling hook
        //normalized vector that from node to test pos
        Vector3 node_to_test = (nextPos - hook.transform.position).normalized;
        Vector3 new_pos = (node_to_test * hookDistance) + hook.transform.position;
        Vector3 new_velocity = new_pos - gameObject.transform.position;

        //force_tension = mass * (d_velo / d_time)
        //where d_velo is new_velocity - old_velocity
        Vector3 delta_velocity = new_velocity - currentVelocityUpf;
        Vector3 tension_force = (rigid.mass * (delta_velocity / Time.fixedDeltaTime));

        rigid.AddForce(tension_force, ForceMode.Impulse);
    }

    #endregion

    private void HandleGround()
    {
        if (IsGrounded)
        { 
            if (usingManGear)
            {
                if (isWaitingToLand && rigid.velocity.magnitude < 3)
                {
                    Land();
                }
                else if (isWaitingToLand && rigid.velocity.magnitude > 3 && !hook)
                {
                    rigid.drag = .5f;
                }
            }
            else
            {
                if (isWaitingToLand)
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

            if (hookStatus != HookStatus.attached)
            {
                usingManGear = true;
            }
        }
    }

    private bool IsGrounded
    {
        get
        {
            var origin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

            if (GameVariables.DEBUG_DRAW_GROUND_CHECK_RAY)
            {
                Debug.DrawLine(origin, (Vector3.down * (coll.bounds.extents.y + .5f)) + origin, Color.red);
            }

            return Physics.Raycast(origin, Vector3.down, coll.bounds.extents.y + .5f, 1);
        }
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

}