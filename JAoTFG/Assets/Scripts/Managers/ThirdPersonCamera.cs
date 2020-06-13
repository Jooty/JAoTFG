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

    private Rigidbody player;

    // locals
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        cam.fieldOfView = GameVariables.FIELD_OF_VIEW;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        var relativePos = transform.position - target.position;
        if (Physics.Raycast(target.position, relativePos, out var hit, distance + .5f, 1))
        {
            distanceOffset = Mathf.Clamp(distance - hit.distance + .8f, 0, distance);
        }
        else
        {
            distanceOffset = 0;
        }

        // Do camera effects
        var fovtarget = Common.GetFloatByRelativePercent(
            GameVariables.FIELD_OF_VIEW,
            GameVariables.FIELD_OF_VIEW * 1.2f, 
            0, 
            GameVariables.HERO_MAX_SPEED, 
            player.velocity.magnitude);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovtarget, .3f);
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

        var dist = Common.GetFloatByRelativePercent(distance, distance * 1.5f, 0, GameVariables.HERO_MAX_SPEED, player.velocity.magnitude);
        var pos = rot * new Vector3(0, 0, -dist + distanceOffset) + target.position;
        transform.rotation = rot;
        transform.position = pos;
    }

}