using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBodyCollider : MonoBehaviour
{

    private CharacterBody charBody;

    private void Start()
    {
        charBody = transform.FindParentWithTag("CharacterBody").GetComponent<CharacterBody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        charBody.ColliderEvent(collision);
    }

}
