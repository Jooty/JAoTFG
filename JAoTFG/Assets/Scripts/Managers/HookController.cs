using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookController : MonoBehaviour
{

    public float speed = 300f;

    public HookStatus status;
    public float tetherDistance;
    public HookSide side;

    [HideInInspector] public PlayerController source;
    [HideInInspector] public Vector3 target;
    [HideInInspector] public bool recall = false;
    [HideInInspector] public LineRenderer grapplingLine;
    [HideInInspector] public Transform eventualParent;

    private bool alreadyCalled = false;

    private void Start()
    {
        grapplingLine = new GameObject("GrapplingLineLeft").AddComponent<LineRenderer>();
        grapplingLine.startWidth = .05f;
        grapplingLine.material.color = Color.black;
        grapplingLine.transform.SetParent(transform);
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

        if (transform.parent != eventualParent || recall)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    private void hookAttached()
    {
        transform.SetParent(eventualParent);
        alreadyCalled = true;
        source.HookAttachedEvent(side);
    }

    private void hookRecalled()
    {
        source.HookRetractedEvent(side);
    }

}
