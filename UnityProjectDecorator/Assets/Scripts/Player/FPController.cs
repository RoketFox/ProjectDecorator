using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
public class FPController : MonoBehaviour
{
    #region Variables

    [SerializeField] private CapsuleCollider skinCollider;

    [SerializeField] private Transform CamPos;
    private Vector3 standCamPos ;
    private Vector3 crouchCamPos;

    [Header("Player info")]
    [SerializeField, ReadOnly] private float currHeight;
    [SerializeField, ReadOnly] private float currRadius;
    [SerializeField, ReadOnly] private float mass;
    [SerializeField] private float normalHeight = 2;
    [SerializeField] private float normalRadius = .5f;
    [SerializeField] private float crouchHeight = 1;
    [SerializeField] private float crouchRadius = .4f;

    [Header("Physics")]
    [SerializeField] private float groundDrag = 0f;
    [SerializeField] private float airDrag    = 0f;

    [Header("Move")]
    [SerializeField, ReadOnly] private float limitSpeed;
    [SerializeField] private float moveSpeed    = 20f;
    [SerializeField] private float runSpeed     = 30f;
    [SerializeField] private float crouchSpeed  = 10f;
    [SerializeField] private float airSpeed     = 10f;
    [SerializeField] private bool holdForRun    = false;
    [SerializeField] private bool holdForCrouch = false;
    private bool isRuning    = false;
    private bool isCrouching = false;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    bool isReadyToJump;

    [Header("Ground")]
    [SerializeField, ReadOnly] bool isGrounded;
    [SerializeField] private float checkOffset = .1f;
    [SerializeField] private float stepOffset  = .5f;
    [SerializeField] private float stepHeight  = .01f;
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

        controller.Player.Run.performed += Run_performed;
        controller.Player.Run.canceled  += Run_canceled;

        controller.Player.Crouch.performed += Crouch_performed;
        controller.Player.Crouch.canceled += Crouch_canceled;

        standCamPos = CamPos.localPosition;
        crouchCamPos = CamPos.localPosition - new Vector3(0 ,0.5f ,0);

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

    private void Start()
    {
    }

    private void Update()
    {
        currHeight = capsColl.height;
        currRadius = capsColl.radius;
        skinColliderCalculate();

        GroundedCalculate();
        if (movInpVec != Vector2.zero) StepHelper();

        Movement();
        SpeedLimiter();

        DragController();

        Crouch();
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

    private void Jump_performed(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
            Jump();
    }

    private void Run_performed(InputAction.CallbackContext ctx)
    {
        if (holdForRun)
            isRuning = true;
        else
            isRuning = !isRuning;
    }

    private void Run_canceled(InputAction.CallbackContext ctx)
    {
        if (holdForRun)
            isRuning = false;
    }

    private void Crouch_performed(InputAction.CallbackContext ctx)
    {
        if (holdForRun)
            isCrouching = true;
        else
            isCrouching = !isCrouching;
    }

    private void Crouch_canceled(InputAction.CallbackContext ctx)
    {
        if (holdForRun)
            isCrouching = false;
    }
    #endregion

    #region Voids
    private void Movement()
    {
        Vector3 moveDir = transform.forward * movInpVec.y + transform.right * movInpVec.x;

        SpeedCalculate();

        rb.AddForce(moveDir * limitSpeed, ForceMode.Force);
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            capsColl.height = crouchHeight;
            capsColl.radius = crouchRadius;
            capsColl.center = new Vector3(0, -0.5f, 0);
            CamPos.localPosition = crouchCamPos;
        }
        else
        {
            capsColl.height = normalHeight;
            capsColl.radius = normalRadius;
            capsColl.center = Vector3.zero;
            CamPos.localPosition = standCamPos;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void DragController()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
            rb.drag = airDrag;
    }

    private void StepHelper()
    {
        if (Physics.Raycast(transform.position - new Vector3(0, currHeight * 0.5f - checkOffset, 0), transform.forward, currRadius + 0.1f, groundMask))
        {
            if (!Physics.Raycast(transform.position - new Vector3(0, currHeight * 0.5f - stepOffset, 0), transform.forward, currRadius + 0.2f, groundMask))
            {
                transform.position += new Vector3(0, stepHeight, 0);
            }
        }
    }
    #endregion

    #region Calculators
    private void skinColliderCalculate()
    {
        skinCollider.center = capsColl.center;
        skinCollider.height = capsColl.height - .05f;
        skinCollider.radius = capsColl.radius + .01f;
    }

    private void SpeedCalculate()
    {
        if (isGrounded)
        {
            if (isCrouching)
                limitSpeed = crouchSpeed;
            else if (isRuning)
                limitSpeed = runSpeed;
            else
                limitSpeed = moveSpeed;
        }
        else
            limitSpeed = airSpeed;
    }

    private void SpeedLimiter()
    {
        Vector3 pVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (pVelocity.magnitude > limitSpeed)
        {
            Vector3 limitVelocity = pVelocity.normalized * limitSpeed;
            rb.velocity = new Vector3(limitVelocity.x, rb.velocity.y, limitVelocity.z);
        }
    }

    #endregion

    #region Utilities
    public void GroundedCalculate()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, currRadius, Vector3.down, out hit, currHeight * 0.5f - currRadius + 0.2f, groundMask))
            isGrounded = true;
        else
            isGrounded = false;
    }
    #endregion
}
