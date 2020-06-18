using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] protected float sprintSpeed = 1.3f;
    [SerializeField] protected float turnSpeed = 5;
    [SerializeField] protected float jumpPower = 5;
    [SerializeField] protected bool canDoubleJump;

    public float currentSpeed;

    // temp bools
    [HideInInspector] protected bool canMove;
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
    public event EventHandler OnLand;

    // locals
    protected CharacterBody characterBody;
    protected Collider Collider;
    protected Rigidbody rigid;

    protected void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
        this.characterBody = GetComponentInChildren<CharacterBody>();
        this.Collider = characterBody.GetComponent<Collider>();

        canMove = true;
    }

    protected void Update()
    {
        currentSpeed = rigid.velocity.magnitude;

        EnforceMaxSpeed();
    }

    public virtual void Move()
    {
        if (!canMove) return;

        OnMove?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Move_AI()
    {
        if (!canMove) return;

        OnMove_AI?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Jump()
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

    public virtual void Land()
    {
        OnLand?.Invoke(this, EventArgs.Empty);

        canJump = true;
        canDoubleJump = true;
        isWaitingToLand = false;

        rigid.drag = 5;
    }

    public virtual void Death()
    {
        OnDeath?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Attack()
    {
        OnAttack?.Invoke(this, EventArgs.Empty);
    }

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

    public abstract void ColliderEvent(Collision coll);

    public bool IsGrounded()
    {
        var origin = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        if (GameVariables.DEBUG_DRAW_GROUND_CHECK_RAY)
        {
            Debug.DrawLine(origin, (Vector3.down * (.4f)) + origin, Color.red);
        }

        return Physics.Raycast(origin, Vector3.down, .4f, 1);
    }

    protected void EnforceMaxSpeed()
    {
        if (rigid.velocity.magnitude > GameVariables.HERO_MAX_SPEED)
        {
            rigid.velocity = rigid.velocity.normalized * GameVariables.HERO_MAX_SPEED;
        }
    }

}
