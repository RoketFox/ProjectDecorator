using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FPController : MonoBehaviour
{

    [Header("Move")]
    [SerializeField] private float moveSpeed  = 20;
    [SerializeField] private float runSpeed   = 30;
    [SerializeField] private float crawlSpeed = 10;
    [SerializeField] private float airSpeed   = 10;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5;
    bool isReadyToJump;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;
    bool isGrounded;

    private InputMaster controller;
    Vector2 movInpVec;
    private Rigidbody rb;
    private CapsuleCollider capsColl;

    private void Awake()
    {
        controller = new InputMaster();

        controller.Player.Movement.performed += Movement_performed;
        controller.Player.Movement.canceled  += Movement_canceled;
        controller.Player.Jump.performed += Jump_performed;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        capsColl = GetComponent<CapsuleCollider>();
    }

    private void OnEnable()
    {
        controller.Player.Enable();
    }

    private void OnDisable()
    {
        controller.Player.Disable();
    }

    private void Movement_performed(InputAction.CallbackContext ctx)
    {
        movInpVec = ctx.ReadValue<Vector2>();
    }

    private void Movement_canceled(InputAction.CallbackContext ctx)
    {
        movInpVec = ctx.ReadValue<Vector2>();
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (isGrounded)
            Jump();
    }

    private void Start()
    {

    }

    private void Update()
    {
        RaycastHit hit;
        isGrounded = Physics.SphereCast(transform.position, capsColl.radius, Vector3.down, out hit, capsColl.height * 0.5f - capsColl.radius + 0.1f, groundMask);

        Movement();
        SpeedLimiter();

        if (isGrounded)
        {
            rb.drag = 5;
        }
        else
            rb.drag = 0;
    }

    private void Movement()
    {
        Vector3 moveDir = transform.forward * movInpVec.y + transform.right * movInpVec.x;

        if (isGrounded)
            rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
    }

    private void SpeedLimiter()
    {
        Vector3 pVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (pVelocity.magnitude > moveSpeed)
        {
            Vector3 limitVelocity = pVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitVelocity.x, rb.velocity.y, limitVelocity.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
}
