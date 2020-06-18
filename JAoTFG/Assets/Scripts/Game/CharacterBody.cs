using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{

    [HideInInspector] public Collider Collider;

    private CharacterController controller;

    private void Awake()
    {
        this.Collider = GetComponent<Collider>();
    }

    private void Start()
    {
        controller = transform.parent.GetComponent<CharacterController>();
    }

    public void ColliderEvent(Collision coll)
    {
        controller.ColliderEvent(coll);
    }

}
