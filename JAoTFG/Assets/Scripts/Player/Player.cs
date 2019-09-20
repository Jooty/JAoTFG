using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Human
{

    public bool hasSwordInHand;

    // Locals
    private PlayerTargets targets;

    private void Awake()
    {
        targets = GetComponent<PlayerTargets>();
    }

    private void Update()
    {
        // TODO
    }

}
