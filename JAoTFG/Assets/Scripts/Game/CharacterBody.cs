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

    public void JumpEvent()
    {
        controller.JumpEvent();
    }

    public void ColliderEvent(Collision coll)
    {

    }

}
