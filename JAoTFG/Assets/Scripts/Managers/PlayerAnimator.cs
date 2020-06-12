using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharaterAnimator
{

    // locals
    private PlayerController playerController;

    new private void Awake()
    {
        this.playerController = GetComponent<PlayerController>();

        base.Awake();
    }

    new private void Start()
    {
        base.animator = GetAnimator();

        base.Start();
    }

    new private void Update()
    {
        base.animator.SetBool("usingManGear", playerController.usingManGear);
        base.animator.SetBool("isHooked", playerController.hooks.Count > 0);
        base.animator.SetBool("isSliding", playerController.isSliding);

        base.Update();
    }

    private Animator GetAnimator()
    {
        return transform.GetChild(0).GetChild(0).GetComponent<Animator>();
    }

}
