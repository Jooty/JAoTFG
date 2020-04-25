using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{

    private HumanoidController controller;

    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    public void JumpEvent(bool isDoubleJump)
    {
        controller.JumpEvent(isDoubleJump);
    }

    public void ColliderEvent(Collision coll)
    {
        controller.ColliderEvent(coll);
    }

}
