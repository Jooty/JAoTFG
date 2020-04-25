using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(Titan))]
public class TitanController : HumanoidController
{

    [SerializeField] [Range(.1f, 1)] private float rotateSpeed = .5f;
    [SerializeField] private float distanceToAttack = 3f;

    private TitanAction action;
    private Player player;

    private int pom;
    private int pissedOffMeter
    {
        get
        {
            return pom;
        }
        set
        {
            if (value > 50) pom = 50;
            else if (value < 0) pom = 0;
            else pom = value;
        }
    }
    private bool foundPlayer;

    // locals
    private Titan titan;
    private NavMeshAgent nav;

    private void Awake()
    {
        titan = GetComponent<Titan>();
        nav = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        SetStartingAction();

        pissedOffMeter = 25;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void Update()
    {
        if (pissedOffMeter >= 25)
        {
            if (!PlayerInSight())
            {
                if (wantsToRotateRight())
                {
                    transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
                }
                else
                {
                    transform.Rotate(0, -(rotateSpeed * Time.deltaTime), 0);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, player.transform.position) > distanceToAttack)
                {
                    nav.SetDestination(player.transform.position);
                }
                else
                {

                }
            }
        }
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

    public override void JumpEvent() { }

    public override void ColliderEvent(Collision coll) { }

    private bool PlayerInSight()
    {
        Vector3 targetLine = player.transform.position - transform.position;
        return Vector3.Dot(targetLine.normalized, transform.forward) < .5f;
    }

    private bool wantsToRotateRight()
    {
        var dir = player.transform.position - transform.position;
        var angle = Vector3.Angle(dir, transform.forward);

        return angle > 1 && angle < 90;
    }

}
