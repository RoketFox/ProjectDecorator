using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Preferences")]
    [SerializeField] private float rotationSpeedX = 0.5f;
    [SerializeField] private float rotationSpeedY = 0.5f;
    [SerializeField] private float stabilization = 0.25f;

    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform neckPos;

    private InputMaster controller;

    float XRotation;
    float _XRotation;
    float YRotation;
    float _YRotation;

    
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
        transform.position = neckPos.position;

        MoveCamera();
    }

    private void MoveCamera()
    {
        XRotation += _XRotation * rotationSpeedX;
        YRotation -= _YRotation * rotationSpeedY;

        YRotation = Mathf.Clamp(YRotation, -90, 90);
        Quaternion newRot = Quaternion.Euler(YRotation, XRotation, 0);
        head.rotation = Quaternion.Slerp(head.rotation, newRot, stabilization);
        body.rotation = Quaternion.Euler(0, XRotation, 0);
    }
}
