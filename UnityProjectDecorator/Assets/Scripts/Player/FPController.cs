using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
public class FPController : MonoBehaviour
{
    #region Variables
    [Header("Player info")]
    [SerializeField, ReadOnly] private float height;
    [SerializeField, ReadOnly] private float radius;
    [SerializeField, ReadOnly] private float mass;

    [Header("Physics")]
    [SerializeField] private float groundDrag = 0f;
    [SerializeField] private float airDrag    = 0f;

    [Header("Move")]
    [SerializeField] private float moveSpeed  = 20f;
    [SerializeField] private float runSpeed   = 30f;
    [SerializeField] private float crawlSpeed = 10f;
    [SerializeField] private float airSpeed   = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    bool isReadyToJump;

    [Header("Ground")]
    [SerializeField] private float checkOffset = 0.1f;
    [SerializeField] private float stepOffset  = 0.5f;
    [SerializeField] private float stepHeight  = 0.1f;
    [SerializeField] private LayerMask groundMask;

    private InputMaster controller;
    Vector2 movInpVec;
    private Rigidbody rb;
    private CapsuleCollider capsColl;
    #endregion

    #region Standart voids
    private void Awake()
    {
        controller = new InputMaster();

        controller.Player.Movement.performed += Movement_performed;
        controller.Player.Movement.canceled  += Movement_canceled;
        controller.Player.Jump.performed += Jump_performed;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        capsColl = GetComponent<CapsuleCollider>();
        height = capsColl.height;
        radius = capsColl.radius;
    }

    private void OnEnable()
    {
        controller.Player.Enable();
    }

    private void OnDisable()
    {
        controller.Player.Disable();
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (movInpVec != Vector2.zero) StepHelper();

        Movement();
        SpeedLimiter();

        DragController();
    }

    private void OnDrawGizmos()
    {
        capsColl = GetComponent<CapsuleCollider>();
        height = capsColl.height;
        radius = capsColl.radius;
    }
    #endregion

    #region Input Actions
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
        if (isGrounded())
            Jump();
    }
    #endregion

    #region Voids
    private void Movement()
    {
        Vector3 moveDir = transform.forward * movInpVec.y + transform.right * movInpVec.x;

        if (isGrounded())
            rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
        else
            rb.AddForce(moveDir * airSpeed, ForceMode.Force);
    }

    private void DragController()
    {
        if (isGrounded())
        {
            rb.drag = groundDrag;
        }
        else
            rb.drag = airDrag;
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

    private void StepHelper()
    {
        if (Physics.Raycast(transform.position - new Vector3(0, height * 0.5f - checkOffset, 0), transform.forward, radius + 0.1f, groundMask))
        {
            if (!Physics.Raycast(transform.position - new Vector3(0, height * 0.5f - stepOffset, 0), transform.forward, radius + 0.2f, groundMask))
            {
                transform.position += new Vector3(0, stepHeight, 0);
            }
        }
    }
    #endregion

    #region Utilities
    public bool isGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, radius, Vector3.down, out hit, height * 0.5f - radius + 0.2f, groundMask);
    }
    #endregion
}
