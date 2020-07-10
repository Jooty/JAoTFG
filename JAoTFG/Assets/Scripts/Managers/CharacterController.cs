using System;
using System.Collections;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float sprintSpeed = 1.3f;
    [SerializeField] protected float turnSpeed = 5;
    [SerializeField] protected float jumpPower = 5;
    public bool canControl;
    [SerializeField] protected bool canDoubleJump;
    protected Vector3 moveInput;
    private PhysicMaterial zfriction;
    private PhysicMaterial mfriction;

    [Header("Abilities")]
    public float ability01CooldownTime;
    public float ability02CooldownTime;
    public float ability03CooldownTime;

    [SerializeField] protected float attackRecoveryTime = 1f;

    public float currentSpeed;

    public bool isAi = false;

    // temp bools
    protected bool canMove;
    protected bool canAttack;
    protected bool canUseAbility01, canUseAbility02, canUseAbility03;

    [HideInInspector] public bool canJump;
    [HideInInspector] public bool jumpedThisFrame;
    [HideInInspector] public bool doubleJumpedThisFrame;
    [HideInInspector] public bool isWaitingToLand;

    // events
    public event EventHandler OnJump;
    public event EventHandler OnMove;
    public event EventHandler OnMove_AI;
    public event EventHandler OnDeath;
    public event EventHandler OnAttack;
    public event EventHandler OnAttackRelease;
    public event EventHandler OnLand;

    // global scripts
    protected AudioManager audioManager;
    protected Camera cam;

    // locals
    protected CharacterBody characterBody;
    protected Collider Collider;
    protected Rigidbody rigid;

    protected void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
        this.characterBody = GetComponentInChildren<CharacterBody>();
        this.Collider = characterBody.GetComponent<Collider>();

        this.audioManager = FindObjectOfType<AudioManager>();
        this.cam = Camera.main;

        canMove = true;
        canAttack = true;
        canJump = true;
        canUseAbility01 = true;
        canUseAbility02 = true;
        canUseAbility03 = true;
    }

    private void Start()
    {
        zfriction = Resources.Load<PhysicMaterial>("Physics/zeroFriction");
        mfriction = Resources.Load<PhysicMaterial>("Physics/maxFriction");
    }

    protected void Update()
    {
        currentSpeed = rigid.velocity.magnitude;

        EnforceMaxSpeed();
        DoControls();
        HandleFriction();
    }

    private void FixedUpdate()
    {
        var input = getMoveInput();
        if (input != Vector3.zero) { Move(input); }
    }

    private void DoControls()
    {
        if (!canControl || isAi) { return; }

        SetMoveInput();

        // Jump
        if (Input.GetButtonDown("Jump")) Jump();
        if (Input.GetButton("Jump")) JumpHold();
        if (Input.GetButtonUp("Jump")) JumpRelease();

        // Attack
        if (Input.GetMouseButtonDown(0)) { Attack(); }
        else if (Input.GetMouseButtonUp(0)) { AttackRelease(); }

        // Abilities
        if (Input.GetButtonDown("Ability_01") && canUseAbility01) Ability_01_Press();
        else if (Input.GetButtonUp("Ability_01")) Ability_01_Release();
        if (Input.GetButtonDown("Ability_02") && canUseAbility02) Ability_02_Press();
        else if (Input.GetButtonUp("Ability_02")) Ability_02_Release();
        if (Input.GetButtonDown("Ability_03") && canUseAbility03) Ability_03_Press();
        else if (Input.GetButtonUp("Ability_03")) Ability_03_Release();
    }

    #region Overrides

    public virtual void Move(Vector3 input)
    {
        if (!canMove || isAi || !canWalk()) return;

        RotateToMovement();

        rigid.AddForce(moveInput * sprintSpeed / Time.deltaTime);

        OnMove?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Move_AI(Vector3 target)
    {
        if (!canMove) return;

        OnMove_AI?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Jump()
    {
        if (!jumpedThisFrame && canJump || canDoubleJump)
        {
            OnJump?.Invoke(this, EventArgs.Empty);

            jumpedThisFrame = true;
            canJump = false;

            if (!IsGrounded() && canDoubleJump)
            {
                canDoubleJump = false;
            }

            var newVelocity = rigid.velocity.ChangeY(jumpPower);
            rigid.velocity = newVelocity;
        }
    }

    public virtual void JumpHold() { }

    public virtual void JumpRelease() { }

    public virtual void Land()
    {
        OnLand?.Invoke(this, EventArgs.Empty);

        canJump = true;
        canDoubleJump = true;
        isWaitingToLand = false;

        rigid.drag = 5;
    }

    public virtual void Death(DeathInfo info)
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Attack()
    {
        OnAttack?.Invoke(this, EventArgs.Empty);
    }

    public virtual void AttackRelease()
    {
        StartCoroutine(attackRecoveryTimer());

        OnAttackRelease?.Invoke(this, EventArgs.Empty);
    }

    protected abstract void Attack_Hit(Collider[] colliders);

    public virtual void HandleGround()
    {
        if (IsGrounded() && isWaitingToLand)
        {
            if (!jumpedThisFrame && currentSpeed < 11f)
            {
                Land();
            }
            else
            {
                rigid.drag = 5;
            }
        }
        else
        {
            rigid.drag = 0.02f;

            isWaitingToLand = true;
            jumpedThisFrame = false;
        }
    }

    public virtual void CharacterBodyColliderEvent(Collision coll) { }

    #region Abilities

    protected virtual void Ability_01_Press()
    {
        if (!canUseAbility01) return;

        StartCoroutine(ability01_Cooldown());
    }

    protected virtual void Ability_01_Release()
    {

    }

    protected virtual void Ability_02_Press()
    {
        if (!canUseAbility02) return;

        StartCoroutine(ability02_Cooldown());
    }

    protected virtual void Ability_02_Release()
    {

    }

    protected virtual void Ability_03_Press()
    {
        if (!canUseAbility03) return;

        StartCoroutine(ability03_Cooldown());
    }

    protected virtual void Ability_03_Release()
    {

    }

    #endregion

    #endregion

    protected void RotateToMovement()
    {
        var _input = getMoveInput().normalized;
        var directionPos = transform.position + (cam.transform.right * _input.x) + (cam.transform.forward * _input.z);
        var dir = directionPos - transform.position;
        dir.y = 0;

        if (moveInput != Vector3.zero)
        {
            if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir)) != 0)
            {
                rigid.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed);
            }
        }
    }

    private void HandleFriction()
    {
        if (!IsGrounded()) return;

        if (moveInput == Vector3.zero)
        {
            Collider.material = mfriction;
        }
        else
        {
            Collider.material = zfriction;
        }
    }

    private void SetMoveInput()
    {
        var input = getMoveInput();
        // add camera relativity
        moveInput = cam.transform.TransformDirection(input.normalized);
        moveInput.y = 0;
        moveInput.Normalize();
    }

    protected void EnforceMaxSpeed()
    {
        if (rigid.velocity.magnitude > Gamerules.HERO_MAX_SPEED)
        {
            rigid.velocity = rigid.velocity.normalized * Gamerules.HERO_MAX_SPEED;
        }
    }

    public bool IsGrounded()
    {
        var origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        Debug.DrawLine(origin, (Vector3.down * (.4f)) + origin, Color.red);

        return Physics.Raycast(origin, Vector3.down, .4f, 1);
    }

    private bool canWalk()
    {
        return IsGrounded() && !isWaitingToLand;
    }

    public Vector3 getMoveInput()
    {
        var _horizontal = Input.GetAxisRaw("Horizontal");
        var _vertical = Input.GetAxisRaw("Vertical");
        return new Vector3(_horizontal, 0, _vertical);
    }

    private IEnumerator attackRecoveryTimer()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackRecoveryTime);

        canAttack = true;
    }

    private IEnumerator ability01_Cooldown()
    {
        canUseAbility01 = false;

        yield return new WaitForSeconds(ability01CooldownTime);

        canUseAbility01 = true;
    }

    private IEnumerator ability02_Cooldown()
    {
        canUseAbility02 = false;

        yield return new WaitForSeconds(ability02CooldownTime);

        canUseAbility02 = true;
    }

    private IEnumerator ability03_Cooldown()
    {
        canUseAbility03 = false;

        yield return new WaitForSeconds(ability03CooldownTime);

        canUseAbility03 = true;
    }
}