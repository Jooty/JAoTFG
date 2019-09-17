using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Human
{

    public bool hasManeuverGear;
    public ManeuverGear maneuverGear;
    public bool usingManeuverGear;
    public bool hasSwordInHand;

    // Locals
    private PlayerTargets targets;

    private void Awake()
    {
        targets = GetComponent<PlayerTargets>();
    }

    private void Start()
    {
        if (hasManeuverGear)
        {
            maneuverGear.gameObject.SetActive(true);
            targets.swordLeft.SetActive(true);
            targets.swordRight.SetActive(true);
        }
        else
        {
            maneuverGear.gameObject.SetActive(false);
            targets.swordLeft.SetActive(false);
            targets.swordRight.SetActive(false);
        }
    }

    private void Update()
    {
        // TODO
    }

}
