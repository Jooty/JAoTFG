using UnityEngine;
using UnityEngine.AI;

public abstract class CharacterAnimator : MonoBehaviour
{
    // locals
    protected CharacterController controller;
    protected Animator animator;
    protected Rigidbody rigid;
    protected NavMeshAgent navAgent;

    protected void Awake()
    {
        this.controller = GetComponent<CharacterController>();
        this.rigid = GetComponent<Rigidbody>();
        this.animator = GetComponentInChildren<Animator>();
        this.navAgent = GetComponent<NavMeshAgent>();
    }

    protected void Start()
    {
        controller.OnMove += Controller_OnMove;
        controller.OnAttack += Controller_OnAttack;
        controller.OnAttackRelease += Controller_OnAttackRelease;
        controller.OnDeath += Controller_OnDeath;
        controller.OnJump += Controller_OnJump;
        controller.OnMove_AI += Controller_OnMove_AI;
        controller.OnLand += Controller_OnLand;
    }

    protected void Update()
    {
        animator.SetFloat("velocity", Common.GetFloatByRelativePercent(0, 1, 0, Gamerules.HERO_MAX_SPEED, rigid.velocity.magnitude));
        animator.SetFloat("velocityY", Common.GetFloatByRelativePercent(0, 1, 0, 9.8f, rigid.velocity.y));
        animator.SetBool("isGrounded", controller.IsGrounded());

        var input = controller.getMoveInput();
        if (input != Vector3.zero)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void Controller_OnLand(object sender, System.EventArgs e)
    {
        animator.SetTrigger("land");
    }

    protected virtual void Controller_OnJump(object sender, System.EventArgs e)
    {
        animator.SetTrigger("jump");
    }

    protected virtual void Controller_OnDeath(object sender, System.EventArgs e)
    {
        animator.SetTrigger("die");
    }

    protected virtual void Controller_OnAttack(object sender, System.EventArgs e)
    {
        animator.SetTrigger("attack");
    }

    protected virtual void Controller_OnAttackRelease(object sender, System.EventArgs e)
    {
        animator.SetTrigger("attackRelease");
    }

    protected virtual void Controller_OnMove(object sender, System.EventArgs e)
    {

    }

    protected virtual void Controller_OnMove_AI(object sender, System.EventArgs e)
    {
        if (navAgent.velocity.magnitude > .1f)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }
}