using System;
using System.Linq;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public HookStatus status;
    public float tetherDistance;
    public HookSide side;

    public event EventHandler OnHookAttached;

    public event EventHandler OnHookRecalled;

    public enum DrawType { ghostLine, realLine }

    [Header("Gizmos")]
    public DrawType drawType = DrawType.ghostLine;

    [HideInInspector] public PlayerController source;
    [HideInInspector] public Vector3 target;
    [HideInInspector] public bool recall = false;
    [HideInInspector] public LineRenderer grapplingLine;
    [HideInInspector] public Transform eventualParent;

    private bool alreadyCalled = false;

    private Transform spawn;
    private Vector3[] linePositions;
    private Vector3[] ghostLinePositions;
    private Transform[] visualizerSpawnPoints;

    // locals
    private LineRenderer lineRenderer;

    private CurvedLineRenderer curvedLineRenderer;

    private void Awake()
    {
        this.lineRenderer = GetComponent<LineRenderer>();
        this.curvedLineRenderer = GetComponent<CurvedLineRenderer>();
    }

    private void FixedUpdate()
    {
        if (status == HookStatus.sheathed) return;

        if (recall)
        {
            if (isNearSpawn())
            {
                hookRecalled();
            }
            else
            {
                Retract();
            }
        }
        else
        {
            if (status == HookStatus.released)
            {
                MovePoints(spawn.position, target);
            }

            if (isNearTarget() && !alreadyCalled)
            {
                hookAttached();
            }
        }
    }

    private void Update()
    {
        if (status == HookStatus.attached)
        {
            linePositions = new Vector3[2] { spawn.position, target };
        }
        else
        {
            linePositions[0] = spawn.position;
        }

        RenderLines();
    }

    public void InitateHook(HookSide side, PlayerController source, Transform spawn, Vector3 target, GameObject hitTarget, Transform[] visualizerSpawnPoints)
    {
        this.side = side;
        this.spawn = spawn;
        this.source = source;
        this.target = target;
        this.visualizerSpawnPoints = visualizerSpawnPoints;

        // create the line
        linePositions = new Vector3[visualizerSpawnPoints.Count() + 1];
        for (int i = 1; i < linePositions.Length; i++)
        {
            linePositions[i] = visualizerSpawnPoints[i - 1].position;
        }
        linePositions[0] = spawn.position;

        status = HookStatus.released;
    }

    private void MovePoints(Vector3 startPos, Vector3 endPos)
    {
        // create ghost line
        Vector3 lineDir = endPos - startPos;
        ghostLinePositions = new Vector3[visualizerSpawnPoints.Length];
        for (int i = 0; i < visualizerSpawnPoints.Count(); i++)
        {
            ghostLinePositions[ghostLinePositions.Length - 1 - i]
                = linePositions[0] + (lineDir.normalized * lineDir.magnitude * (1 - Mathf.Clamp01((float)i / (float)visualizerSpawnPoints.Count())));
        }

        // move all points at once evenly
        for (int i = 1; i < linePositions.Length; i++)
        {
            linePositions[i] = Vector3.MoveTowards(linePositions[i], ghostLinePositions[i - 1], Time.deltaTime * 95);
        }
    }

    private void Retract()
    {
        status = HookStatus.retracting;

        for (int i = 0; i < linePositions.Length; i++)
        {
            float speed = 100 * Time.deltaTime;
            linePositions[i] = Vector3.MoveTowards(linePositions[i], spawn.position, speed);
        }
    }

    private void RenderLines()
    {
        curvedLineRenderer.SetPoints(linePositions);
    }

    private void hookAttached()
    {
        alreadyCalled = true;
        tetherDistance = Vector3.Distance(getLastPoint(), source.transform.position);
        status = HookStatus.attached;

        // ghostLinePositions = null;

        OnHookAttached?.Invoke(this, EventArgs.Empty);
    }

    private void hookRecalled()
    {
        status = HookStatus.sheathed;

        OnHookRecalled?.Invoke(this, EventArgs.Empty);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (ghostLinePositions == null || linePositions == null) return;

        // draw ghost line
        if (drawType == DrawType.ghostLine)
        {
            for (int i = 0; i < ghostLinePositions.Count(); i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(ghostLinePositions[i], .1f);
                if (ghostLinePositions.Count() - 1 == i)
                {
                    return;
                }
                else if (i == 0)
                {
                    Gizmos.DrawLine(spawn.position, ghostLinePositions[0]);
                }
                else
                {
                    Gizmos.DrawLine(ghostLinePositions[i], ghostLinePositions[i + 1]);
                }
            }
        }
        else
        {
            for (int i = 0; i < linePositions.Count(); i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(linePositions[i], .1f);
                if (linePositions.Count() - 1 == i)
                {
                    return;
                }
                else if (i == 0)
                {
                    Gizmos.DrawLine(spawn.position, linePositions[0]);
                }
                else
                {
                    Gizmos.DrawLine(linePositions[i], linePositions[i + 1]);
                }
            }
        }
    }

    private bool isNearTarget()
    {
        return Vector3.Distance(getLastPoint(), target) < 1;
    }

    private bool isNearSpawn()
    {
        return Vector3.Distance(getLastPoint(), spawn.position) < 1;
    }

    public Vector3 getLastPoint()
    {
        return linePositions[linePositions.Count() - 1];
    }
}