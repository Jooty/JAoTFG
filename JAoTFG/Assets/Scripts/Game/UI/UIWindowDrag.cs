using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowDrag : MonoBehaviour
{

    public void Drag()
    {
        transform.position = Input.mousePosition;
    }

}
