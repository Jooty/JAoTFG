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

    private PhysicMaterial zfriction;
    private PhysicMaterial mfriction;

    private Camera cam;

    private Vector3 directionPos;
    private Vector3 moveInput;

    private bool isSprinting;
    private bool isWaitingToLand;
    private bool canJump;
    private bool wantsToJump;

    private Animator bodyAnim;

    // local components
    private Rigidbody rigid;
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
    }

    private void Update()
    {
        HandleFriction();
        HandleGround();
        HandleControls();
        HandleAnimations();
        EnforceMaxSpeed();

        if (player.hasManeuverGear)
        {
            ManueverGearUpdate();
        }
    }

    void FixedUpdate()
    {
        if (player.usingManeuverGear)
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
        bodyAnim.SetFloat("fallSpeed", rigid.velocity.y);
        bodyAnim.SetBool("isHooked", player.maneuverGear.hookStatus == ManeuverGear.HookStatus.attached);

        if (!player.usingManeuverGear)
        {
            //anim.SetLayerWeight(0, 1);
            //anim.SetLayerWeight(2, 0);

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
            //anim.SetLayerWeight(0, 1);
            //anim.SetLayerWeight(2, 0);
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
        canJump = false;

        // TEMP -- REMOVE LATER
        if (player.maneuverGear && player.maneuverGear.hookStatus != ManeuverGear.HookStatus.sheathed)
        {
            player.usingManeuverGear = true;
            rigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
        else
        {
            rigid.AddForce(transform.up * jumpPower, ForceMode.Impulse);
        }
    }

    private void JumpEvent()
    {
        if (player.maneuverGear && player.maneuverGear.hookStatus != ManeuverGear.HookStatus.sheathed)
        {
            player.usingManeuverGear = true;
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

        if (player.maneuverGear)
        {
            player.usingManeuverGear = false;
        }
    }

    private void HandleFriction() 
    {
        if (player.usingManeuverGear) return;

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
        if (!player.hasManeuverGear) return;

        UpdateManeuverGearCameraEffects();
        UpdateManeuverGearUI();
        CheckHookRunawayDistance();
        HandleManeuverGearControls();

        if (!player.usingManeuverGear && player.maneuverGear.hook) 
        {
            // todo
            player.maneuverGear.hookDistance 
                = Vector3.Distance(transform.position, player.maneuverGear.hook.transform.position);
        }

        if (player.usingManeuverGear)
        { 
            AirRotate();
        }
    }

    private void HandleManeuverGearControls()
    {
        if (!player.hasManeuverGear) return;

        if (Input.GetMouseButton(0) && !player.maneuverGear.hook && CanHook())
        {
            FireHook();
        }
        else if (Input.GetMouseButtonUp(0) && player.maneuverGear.hook)
        {
            RecallHook();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (player.maneuverGear.hasSwordInHand)
            {
                SheatheSwords();
            }
            else
            {
                UnsheatheSwords();
            }
        }

        if (Input.GetMouseButtonDown(0) && player.maneuverGear.hasSwordInHand)
        {
            SwordReady();
        }
        else if (Input.GetMouseButtonUp(0) && player.maneuverGear.hasSwordInHand)
        {
            SwordRelease();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (IsGrounded && !player.maneuverGear.hook) return;

            GasThrust();

            if (!aud.isPlaying)
            {
                aud.clip = player.maneuverGear.manSoundEffects[3];
                aud.loop = true;
                aud.Play();
            }
            player.maneuverGear.thrustSmoke.Play();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            player.maneuverGear.isThrusting = false;
            aud.Stop();
            player.maneuverGear.thrustSmoke.Pause();
        }
    }

    private void UpdateManeuverGearUI()
    {
        if (CanHook())
        {
            player.maneuverGear.hookUI.SetActive(true);
        }
        else
        {
            player.maneuverGear.hookUI.SetActive(false);
        }
    }

    private void UpdateManeuverGearCameraEffects()
    {
        if (!player.usingManeuverGear) return;

        var target = Common.GetFloatByRelativePercent(GameVariables.FIELD_OF_VIEW, GameVariables.FIELD_OF_VIEW * 1.2f, 0, GameVariables.HERO_MAX_SPEED, rigid.velocity.magnitude);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, .3f);
    }

    /// <summary>
    /// Check to see if the player has run far away from the max hook distance.
    /// If so, break the tether.
    /// </summary>
    private void CheckHookRunawayDistance()
    {
        if (!player.maneuverGear.hook) return;

        if (Vector3.Distance(transform.position, player.maneuverGear.hook.transform.position) > GameVariables.MG_HOOK_MAX_RUNAWAY_RANGE)
        {
            RecallHook();
        }
    }

    private void GasThrust()
    {
        rigid.AddForce(transform.forward * player.maneuverGear.thrustPower * Time.deltaTime, ForceMode.Acceleration);
        player.maneuverGear.isThrusting = true;

        if (player.maneuverGear.hook && GameVariables.MG_RETRACT_ON_GAS)
        {
            player.maneuverGear.hookDistance -= GameVariables.MG_GAS_REEL_SPEED * Time.deltaTime;
        }

        player.maneuverGear.gas -= player.maneuverGear.gasReduceSpeed * Time.deltaTime;
    }

    private void UnsheatheSwords()
    {
        player.maneuverGear.sword.SetActive(true);
        player.maneuverGear.sword2.SetActive(true);

        bodyAnim.SetTrigger("withdrawSword");
        player.maneuverGear.hasSwordInHand = true;

        aud.PlayOneShot(player.maneuverGear.manSoundEffects[0], .2f);
    }

    private void SheatheSwords()
    {
        player.maneuverGear.sword.SetActive(false);
        player.maneuverGear.sword2.SetActive(false);

        bodyAnim.SetTrigger("sheatheSword");
        player.maneuverGear.hasSwordInHand = false;
    }

    private void SwordReady()
    {
        bodyAnim.SetTrigger("attack");
    }

    private void SwordRelease()
    {
        bodyAnim.SetTrigger("attackRelease");
        aud.PlayOneShot(player.maneuverGear.manSoundEffects[11], AudioSettings.SFX);
        player.maneuverGear.sword.GetComponent<BoxCollider>().enabled = true;
        player.maneuverGear.sword2.GetComponent<BoxCollider>().enabled = true;
        StartCoroutine(DisableSwords());
    }

    private IEnumerator DisableSwords()
    {
        yield return new WaitForSeconds(2);
        player.maneuverGear.sword.GetComponent<BoxCollider>().enabled = false;
        player.maneuverGear.sword2.GetComponent<BoxCollider>().enabled = false;
    }

    private void AirRotate()
    {
        Quaternion target = Quaternion.identity;

        if (player.maneuverGear.hookStatus == ManeuverGear.HookStatus.attached && rigid.velocity.magnitude > 3)
        {
            var tetherDirection = player.maneuverGear.hook.transform.position - transform.position;
            target = Quaternion.LookRotation(rigid.velocity, tetherDirection);
        }
        else
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

    private void FireHook()
    {
        // SFX
        // player.maneuverGear.hookSmoke.Play();
        aud.PlayOneShot(player.maneuverGear.manSoundEffects[10]);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, GameVariables.MG_HOOK_RANGE, 1))
        {
            if (rigid.velocity.y < -5)
            {
                player.usingManeuverGear = true;
            }

            var _hook = Instantiate(Resources.Load<GameObject>("Hook"), transform.position, transform.rotation);
            player.maneuverGear.hook = _hook.GetComponent<HookController>();
            player.maneuverGear.hook.target = hit.point;
            player.maneuverGear.hook.source = player.maneuverGear;
            player.maneuverGear.hookStatus = ManeuverGear.HookStatus.released;

            player.maneuverGear.grapplingLine.SetActive(true);
        }
    }

    private void RecallHook()
    {
        player.maneuverGear.hookStatus = ManeuverGear.HookStatus.retracting;

        // sound effect
        aud.PlayOneShot(player.maneuverGear.manSoundEffects[9]);

        player.maneuverGear.hook.recall = true;

        if (IsGrounded)
        {
            player.usingManeuverGear = false;
        }

        return;
    }

    private void DoManeuverGearPhysics()
    {
        if (Input.GetMouseButton(0) && player.maneuverGear.hookStatus == ManeuverGear.HookStatus.attached)
        {
            // gets velocity in units/frame, then gets the position for next frame
            Vector3 curr_velo_upf = rigid.velocity * Time.fixedDeltaTime;
            Vector3 test_pos = transform.position + curr_velo_upf;

            if (Vector3.Distance(test_pos, player.maneuverGear.hook.transform.position) < player.maneuverGear.hookDistance)
            {
                player.maneuverGear.hookDistance = Vector3.Distance(test_pos, player.maneuverGear.hook.transform.position);
            }

            ApplyTensionForce(curr_velo_upf, test_pos);
        }

        return;
    }

    private void ApplyTensionForce(Vector3 curr_velo_upf, Vector3 test_pos)
    {
        if (player.maneuverGear.hookStatus != ManeuverGear.HookStatus.attached) return;

        //finds what the new velocity is due to tension force grappling hook
        //normalized vector that from node to test pos
        Vector3 node_to_test = (test_pos - player.maneuverGear.hook.transform.position).normalized;
        Vector3 new_pos = (node_to_test * player.maneuverGear.hookDistance) + player.maneuverGear.hook.transform.position;
        Vector3 new_velocity = new_pos - gameObject.transform.position;

        //force_tension = mass * (d_velo / d_time)
        //where d_velo is new_velocity - old_velocity
        Vector3 delta_velocity = new_velocity - curr_velo_upf;
        Vector3 tension_force = (rigid.mass * (delta_velocity / Time.fixedDeltaTime));

        rigid.AddForce(tension_force, ForceMode.Impulse);
    }

    #endregion

    private void HandleGround()
    {
        if (IsGrounded)
        { 
            if (player.usingManeuverGear)
            {
                if (isWaitingToLand && rigid.velocity.magnitude < 3)
                {
                    Land();
                }
                else if (isWaitingToLand && rigid.velocity.magnitude > 3)
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

            if (player.maneuverGear.hookStatus != ManeuverGear.HookStatus.attached)
            {
                player.usingManeuverGear = true;
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