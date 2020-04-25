using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Titan : MonoBehaviour
{

    private bool isDead = false;

    [SerializeField] private Transform characterBody;

    // locals
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();

        SetRandomSize();
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

    private void SetRandomSize()
    {
        var min = GameVariables.TITAN_MIN_SIZE;
        var max = GameVariables.TITAN_MAX_SIZE;
        var yRandom = Random.Range(min, max);
        var relativeScale = Common.GetFloatByRelativePercent(characterBody.localScale.x, GameVariables.TITAN_MAX_X_SIZE_GROWTH, min, max, yRandom);

        characterBody.localScale = new Vector3(relativeScale, yRandom, relativeScale);
    }

    private IEnumerator StartDissipateTimer()
    {
        yield return new WaitForSeconds(GameVariables.TITAN_DISSIPATE_TIMER);

        Destroy(gameObject);
    }

}
