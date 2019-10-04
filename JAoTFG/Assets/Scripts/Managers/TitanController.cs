using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Titan))]
public class TitanController : HumanoidController
{

    private TitanAction action;

    private Player player;

    // locals
    private Titan titan;

    private void Awake()
    {
        titan = GetComponent<Titan>();
    }

    private void Start()
    {
        SetStartingAction();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public override void JumpEvent()
    {

    }

    public override void ColliderEvent(Collision coll)
    {
        throw new System.NotImplementedException();
    }

    private void SetStartingAction()
    {
        var r = Random.value > .5f;
        if (r)
        {
            action = TitanAction.sit;
        }
        else
        {
            action = TitanAction.idle;
        }
    }

}
