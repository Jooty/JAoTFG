using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TitanController : CharacterController
{
    public float eyeSlashRecoveryTime = 20f;
    public float ankleSlashRecoveryTime = 20f;

    private TitanBodyHitbox[] hitboxes;

    private PlayerController player;

    // locals
    private TitanAnimator animator;

    private NavMeshAgent agent;

    private new void Awake()
    {
        this.animator = GetComponent<TitanAnimator>();
        this.hitboxes = GetComponentsInChildren<TitanBodyHitbox>();
        this.agent = GetComponent<NavMeshAgent>();

        base.Awake();
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    private new void Update()
    {
        base.Update();

        Move_AI(player.transform.position);
    }

    public override void Move_AI(Vector3 target)
    {
        base.Move_AI(target);

        if (agent.enabled)
        {
            agent.SetDestination(target);
        }
    }

    protected override void Attack_Hit(Collider[] colliders)
    {
        throw new System.NotImplementedException();
    }

    public void HitboxHitEvent(TitanBodyHitboxType type)
    {
        switch (type)
        {
            case TitanBodyHitboxType.ankle:
                SlashAnkles();
                break;

            case TitanBodyHitboxType.eyes:
                SlashEyes();
                break;

            case TitanBodyHitboxType.nape:
                base.Death();
                break;
        }
    }

    public override void CharacterBodyColliderEvent(Collision coll)
    {
        throw new System.NotImplementedException();
    }

    private void SlashAnkles()
    {
        StartCoroutine(RecoveryTimer(ankleSlashRecoveryTime));
    }

    private void SlashEyes()
    {
        StartCoroutine(RecoveryTimer(eyeSlashRecoveryTime));
    }

    private IEnumerator RecoveryTimer(float timeToWait)
    {
        base.canMove = false;
        yield return new WaitForSeconds(timeToWait);
        base.canMove = true;
    }
}