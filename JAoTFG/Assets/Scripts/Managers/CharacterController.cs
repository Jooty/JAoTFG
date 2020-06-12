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

    public float speed;

    public bool canJump;
    public bool jumpedThisFrame;
    public bool doubleJumpedThisFrame;
    public bool isWaitingToLand;

    public event EventHandler OnJump;
    public event EventHandler OnMove;
    public event EventHandler OnMove_AI;
    public event EventHandler OnDeath;
    public event EventHandler OnAttack;
    public event EventHandler OnLand;

    // locals
    [SerializeField] protected CapsuleCollider coll;
    protected Rigidbody rigid;

    protected void Awake()
    {
        this.rigid = GetComponent<Rigidbody>();
    }

    protected void Update()
    {
        speed = rigid.velocity.magnitude;

        EnforceMaxSpeed();
    }

    public virtual void Move()
    {
        OnMove?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Move_AI()
    {
        OnMove_AI?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Jump()
    {
        jumpedThisFrame = true;
        canJump = false;

        if (!IsGrounded() && canDoubleJump)
        {
            canDoubleJump = false;
        }

        var newVelocity = rigid.velocity.ChangeY(jumpPower);
        rigid.velocity = newVelocity;

        OnJump?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Land()
    {
        canJump = true;
        canDoubleJump = true;
        isWaitingToLand = false;

        rigid.drag = 5;

        OnLand?.Invoke(this, EventArgs.Empty);
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
            if (!jumpedThisFrame && speed < 11f)
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
            Debug.DrawLine(origin, (Vector3.down * (coll.bounds.extents.y + .3f)) + origin, Color.red);
        }

        return Physics.Raycast(origin, Vector3.down, coll.bounds.extents.y + .3f, 1);
    }

    protected void EnforceMaxSpeed()
    {
        if (rigid.velocity.magnitude > GameVariables.HERO_MAX_SPEED)
        {
            rigid.velocity = rigid.velocity.normalized * GameVariables.HERO_MAX_SPEED;
        }
    }

}
