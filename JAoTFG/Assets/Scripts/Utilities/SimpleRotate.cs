using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{

    public float speed = 2f;

    void Update()
    {
        transform.Rotate(0, Time.deltaTime * speed, 0, Space.Self);
    }

}
