using UnityEngine;

public class UIWindowDrag : MonoBehaviour
{
    public void Drag()
    {
        transform.position = Input.mousePosition;
    }
}