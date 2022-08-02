using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum HeadbobStates {idle, walk, run, crouch}
    public HeadbobStates currHbState = HeadbobStates.idle;

    [Header("Preferences")]
    [SerializeField] private bool cursorLocked     = true;
    [SerializeField] private bool camRotateEnabled = true;
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform neckPos;
    [SerializeField] private float rotationSpeedX = 0.3f;
    [SerializeField] private float rotationSpeedY = 0.3f;
    [SerializeField] private float stabilization  = 0.2f;

    [Header("HeadBob")]
    [SerializeField] private bool headbobEnabled = true;
    [SerializeField, ReadOnly] private float currAmplitude = 0;
    [SerializeField, ReadOnly] private float currFrequency = 0;
    [SerializeField] private float walkAmplitude   = 1f;
    [SerializeField] private float walkFrequency   = 2f;
    [SerializeField] private float runAmplitude    = 1f;
    [SerializeField] private float runFrequency    = 2f;
    [SerializeField] private float crouchAmplitude = 1f;
    [SerializeField] private float crouchFrequency = 2f;
    private Vector3 startPos;
    private Vector3 camPos = Vector3.zero;

    private float XRotation;
    private float _XRotation;
    private float YRotation;
    private float _YRotation;

    

    private InputMaster controller;


    
    private void Awake()
    {
        startPos = transform.localPosition;

        controller = new InputMaster();

        controller.Player.Mouse.performed += Mouse_performed;
        controller.Player.Mouse.canceled += Mouse_canceled;

    }
    private void OnEnable()
    {
        if (cursorLocked)
            Cursor.lockState = CursorLockMode.Locked;

        controller.Player.Enable();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
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

        if (headbobEnabled)
        {
        CheckHbState();
        HeadBob(currAmplitude, currFrequency);
        }
        ResetPosition();

        if(camRotateEnabled)
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
        BobAmplitude = BobAmplitude / 100;

        camPos = Vector3.zero;
        camPos.x += Mathf.Sin(Time.time * BobFrequency / 2) * BobAmplitude * 2;
        camPos.y += Mathf.Cos(Time.time * BobFrequency) * BobAmplitude;
        camPos.z += Mathf.Cos(Time.time * BobFrequency / 3) * BobAmplitude / 3;

        transform.localPosition += (camPos);
    }

    private void ResetPosition()
    {
        if (transform.localPosition != startPos)
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, 1 * Time.deltaTime);
    }

    private void CheckHbState()
    {
        switch (currHbState)
        {
            case HeadbobStates.idle:
                currAmplitude = 0;
                currFrequency = 0;
                break;

            case HeadbobStates.walk:
                currAmplitude = walkAmplitude;
                currFrequency = walkFrequency;
                break;

            case HeadbobStates.run:
                currAmplitude = runAmplitude;
                currFrequency = runFrequency;
                break;

            case HeadbobStates.crouch:
                currAmplitude = crouchAmplitude;
                currFrequency = crouchFrequency;
                break;
        }
    }
}
