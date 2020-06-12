using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharaterAnimator
{

    // locals
    private PlayerController playerController;

    private void Awake()
    {
        this.playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        base.animator = GetAnimator();
    }

    private void Update()
    {
        base.animator.SetBool("usingManGear", playerController.usingManGear);
        base.animator.SetBool("isHooked", playerController.hooks.Count > 0);
        base.animator.SetBool("isSliding", playerController.isSliding);
    }

    private Animator GetAnimator()
    {
        return transform.GetChild(0).GetChild(0).GetComponent<Animator>();
    }

}
