using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan : MonoBehaviour
{

    private bool isDead = false;

    // locals
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        anim.SetTrigger("die");
        GetComponent<MeshCollider>().enabled = false;

        if (GameManager.instance.gamemode == Gamemode.arena)
        {
            ArenaManager.instance.RemoveTitan(this);
        }

        StartCoroutine(StartDissipateTimer());
    }

    private IEnumerator StartDissipateTimer()
    {
        yield return new WaitForSeconds(GameVariables.TITAN_DISSIPATE_TIMER);

        Destroy(gameObject);
    }

}
