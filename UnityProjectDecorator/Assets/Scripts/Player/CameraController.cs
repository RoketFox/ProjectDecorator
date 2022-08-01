using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform neckPos;
    [SerializeField] private float rotationSpeedX = 0.3f;
    [SerializeField] private float rotationSpeedY = 0.3f;
    [SerializeField] private float stabilization  = 0.2f;
    [Header("HeadBob")]
    [SerializeField] private float walkAmplitude   = 0.01f;
    [SerializeField] private float walkFrequency   = 0.02f;
    [SerializeField] private float runAmplitude    = 0.01f;
    [SerializeField] private float runFrequency    = 0.02f;
    [SerializeField] private float crouchAmplitude = 0.01f;
    [SerializeField] private float crouchFrequency = 0.02f;

    Vector3 camPos = Vector3.zero;
    float XRotation;
    float _XRotation;
    float YRotation;
    float _YRotation;

    

    private InputMaster controller;


    
    private void Awake()
    {
        controller = new InputMaster();

        controller.Player.Mouse.performed += Mouse_performed;
        controller.Player.Mouse.canceled += Mouse_canceled;

    }
    private void OnEnable()
    {
        controller.Player.Enable();
    }

    private void OnDisable()
    {
        controller.Player.Disable();
    }
    public void Mouse_performed(InputAction.CallbackContext ctx)
    {
        _XRotation = ctx.ReadValue<Vector2>().x;
        _YRotation = ctx.ReadValue<Vector2>().y;
    }

    private void Mouse_canceled(InputAction.CallbackContext obj)
    {
        _XRotation = 0;
        _YRotation = 0;
    }

    private void Start()
    {
        if (!head)
        {
            head = transform;
        }
        if (!body)
        {
            body = transform;
        }
        if (!neckPos)
        {
            neckPos = transform;
        }

    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {

        head.transform.position = neckPos.position;

        RotateCamera();
    }
    private void RotateCamera()
    {
        XRotation += _XRotation * rotationSpeedX;
        YRotation -= _YRotation * rotationSpeedY;

        YRotation = Mathf.Clamp(YRotation, -90, 90);
        Quaternion newRot = Quaternion.Euler(YRotation, XRotation, 0);
        head.rotation = Quaternion.Slerp(head.rotation, newRot, stabilization);
        body.rotation = Quaternion.Euler(0, XRotation, 0);
    }

    private void HeadBob(float BobAmplitude, float BobFrequency)
    {
        camPos.x = transform.localPosition.x + Mathf.Sin(Time.time * BobFrequency / 2) * BobAmplitude * 2;
        camPos.y = transform.localPosition.y + Mathf.Cos(Time.time * BobFrequency) * BobAmplitude;
        camPos.z = transform.localPosition.y + Mathf.Cos(Time.time * BobFrequency / 5) * BobAmplitude / 5;

        transform.localPosition = new Vector3(camPos.x, camPos.y, camPos.z);
    }
}
