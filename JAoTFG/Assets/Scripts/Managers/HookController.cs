using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookController : MonoBehaviour
{

    public float speed = 100f;
    public float retractSpeed = 300f;

    [HideInInspector]
    public PlayerController source;

    [HideInInspector]
    public Vector3 target;

    [HideInInspector]
    public bool hooked = false;

    [HideInInspector]
    public bool recall = false;

    private bool alreadyCalled = false;

    private Rigidbody sourceRigid;

    private void Start()
    {
        if (source)
        {
            sourceRigid = source.GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        if (recall)
        {
            target = source.transform.position;

            if (Vector3.Distance(transform.position, source.transform.position) < 2)
            {
                hookRecalled();
            }
        }

        if (transform.position == target && !alreadyCalled)
        {
            hookAttached();
        }

        transform.position = Vector3.MoveTowards(transform.position, target, (recall ? source.hookReturnSpeed : source.hookSpeed) * Time.deltaTime);
    }

    private void hookAttached()
    {
        source.hookStatus = HookStatus.attached;
        source.HookAttachedEvent();

        var clip = Resources.Load<AudioClip>("SFX/HERO/HookHit");
        // todo
        // GetComponent<AudioSource>().PlayOneShot(clip, AudioSettings.SFX);
    }

    private void hookRecalled()
    {
        source.hookStatus = HookStatus.sheathed;

        source.grapplingLine.SetActive(false);

        Destroy(gameObject);
    }

}
