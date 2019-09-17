using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitanNape : MonoBehaviour
{

    private Titan titan;

    private void Start()
    {
        titan = transform.FindParentWithTag("Titan").GetComponent<Titan>();
    }

    public void Hit()
    {
        titan.Die();
    }

}
