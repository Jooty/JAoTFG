using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBody : MonoBehaviour
{

    private CharacterController controller;

    private void Start()
    {
        controller = transform.parent.GetComponent<CharacterController>();
    }

    public void ColliderEvent(Collision coll)
    {
        controller.ColliderEvent(coll);
    }

}
