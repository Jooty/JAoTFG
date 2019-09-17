using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour {

    public bool lockCursor;
    public float mouseSensitivity = 10;
    public Transform target;
    public float distance = 5;
    public float distanceOffset;
    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = .12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    public bool DoRotate = true;

    float yaw;
    float pitch;

    private void Start()
    {
        this.GetComponent<Camera>().fieldOfView = GameVariables.FIELD_OF_VIEW;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        var relativePos = transform.position - target.position;
        if (Physics.Raycast(target.position, relativePos, out var hit, distance + .5f))
        {
            distanceOffset = Mathf.Clamp(distance - hit.distance + .8f, 0, distance);
        }
        else
        {
            distanceOffset = 0;
        }
    }

    private void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        //if (DoRotate)
        //{
        //    currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        //    transform.eulerAngles = currentRotation;
        //}

        var rot = Quaternion.Euler(pitch, yaw, 0);
        var pos = rot * new Vector3(0, 0, -distance + distanceOffset) + target.position;
        transform.rotation = rot;
        transform.position = pos;
    }

}