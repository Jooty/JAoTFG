using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Titan))]
public class TitanController : MonoBehaviour
{

    private TitanAction action;
    private bool isLazyTitan;

    private Player currentTarget;

    // locals
    private Titan titan;

    private void Awake()
    {
        titan = GetComponent<Titan>();
    }

    private void Start()
    {
        action = TitanAction.idle;
        Boolean.SetBooleanRandom(ref isLazyTitan);
    }

}
