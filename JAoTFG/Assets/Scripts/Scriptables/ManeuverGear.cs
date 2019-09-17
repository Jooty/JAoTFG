using UnityEngine;

public class ManeuverGear : Gear
{

    public float gas, totalMaxGas;
    public float burstGas, maxBurstGas;
    public float gasReduceSpeed;
    public float burstGasRechargeSpeed;
    public float burstGasReduction;
    public float thrustPower;
    public float burstPower;
    public float hookSpeed, hookReturnSpeed;

    [SerializeField] public GameObject hookUI;
    [SerializeField] public GameObject sword;
    [SerializeField] public GameObject sword2;
    [SerializeField] public ParticleSystem thrustSmoke;
    [SerializeField] public ParticleSystem thrustBurstSmoke;
    [SerializeField] public ParticleSystem hookSmoke;
    [SerializeField] public Transform leftHookPoint;

    [HideInInspector] public bool hasSwordInHand;
    [HideInInspector] public bool isThrusting;

    [HideInInspector] public HookController hook;
    [HideInInspector] public GameObject grapplingLine;
    [HideInInspector] public enum HookStatus { sheathed, released, attached, retracting };
    [HideInInspector] public HookStatus hookStatus;
    [HideInInspector] public float hookDistance;

    [HideInInspector] public AudioClip[] manSoundEffects;

    private void Awake()
    {
        gas = totalMaxGas;
        burstGas = maxBurstGas;
    }

    private void Start()
    {
        // draw hook
        grapplingLine = new GameObject("GrapplingLine");
        grapplingLine.SetActive(false);
        grapplingLine.transform.SetParent(leftHookPoint);
        LineRenderer line_renderer = grapplingLine.AddComponent<LineRenderer>();
        line_renderer.startWidth = .05f;
        line_renderer.material.color = Color.black;

        manSoundEffects = Resources.LoadAll<AudioClip>("SFX/HERO");
    }

    private void Update()
    {
        DrawHook();
    }

    private void DrawHook()
    {
        if (!hook) return;

        Vector3[] line_vortex_arr = { leftHookPoint.position, hook.transform.position };
        grapplingLine.GetComponent<LineRenderer>().SetPositions(line_vortex_arr);
    }

    public void HookAttachedEvent()
    {
        hookStatus = HookStatus.attached;

        hookDistance = Vector3.Distance(hook.transform.position, gameObject.transform.position);
    }

}