using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharaterAnimator : MonoBehaviour
{

    // locals
    protected CharacterController controller;
    protected Animator animator;
    protected Rigidbody rigid;

    private void Awake()
    {
        this.controller = GetComponent<CharacterController>();
        this.rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        controller.OnMove += Controller_OnMove;
        controller.OnAttack += Controller_OnAttack;
        controller.OnDeath += Controller_OnDeath;
        controller.OnJump += Controller_OnJump;
        controller.OnMove_AI += Controller_OnMove_AI;
        controller.OnLand += Controller_OnLand;
    }

    private void Controller_OnLand(object sender, System.EventArgs e)
    {
        animator.SetTrigger("land");
    }

    private void Update()
    {
        animator.SetFloat("velocity", Common.GetFloatByRelativePercent(0, 1, 0, GameVariables.HERO_MAX_SPEED, rigid.velocity.magnitude));
        animator.SetFloat("velocityY", Common.GetFloatByRelativePercent(0, 1, 0, 9.8f, rigid.velocity.y));
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

    protected virtual void Controller_OnMove(object sender, System.EventArgs e)
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    protected virtual void Controller_OnMove_AI(object sender, System.EventArgs e)
    {

    }

}
