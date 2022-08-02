using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
public class FPController : MonoBehaviour
{
    #region Variables
    private InputMaster controller;

    [Header("Player info")]
    [SerializeField] private CapsuleCollider skinCollider;
    private CapsuleCollider capsColl;
    [SerializeField, ReadOnly] private float currHeight;
    [SerializeField, ReadOnly] private float currRadius;
    [SerializeField] private float normalHeight = 2;
    [SerializeField] private float normalRadius = .5f;
    [SerializeField] private float crouchHeight = 1;
    [SerializeField] private float crouchRadius = .49f;

    [Header("Camera")]
    [SerializeField] private CameraController camController;
    [SerializeField] private Transform neck;
    [SerializeField] private float standCamOffset  = .6f;
    [SerializeField] private float crouchCamOffset = -.1f;
    private Vector3 currCamPos;
    [SerializeField] private float camToCrouchSpeed = 5;

    [Header("Physics")]
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag    = 2f;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] bool movementEnabled = true;
    private Vector2 movInpVec;
    [SerializeField, ReadOnly] public float currentSpeed;
    [SerializeField] private float walkSpeed    = 20f;
    [SerializeField] private float airSpeed     = 5f;
    [SerializeField] private float runSpeed     = 30f;
    [SerializeField] private bool holdForRun    = false;
    private bool isRuning    = false;
    [SerializeField] private float crouchSpeed  = 10f;
    [SerializeField] private bool holdForCrouch = false;
    private bool isCrouching = false;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    private bool isReadyToJump;

    [Header("Ground")]
    [SerializeField, ReadOnly] bool isGrounded;
    [SerializeField] private LayerMask groundMask;

    [Header("Step system")]
    [SerializeField] private float lowerOffset   = .01f;
    [SerializeField] private float lowerLength   = .2f;
    [SerializeField] private float higherOffset  = .5f;
    [SerializeField] private float higherLength  = .5f;
    [SerializeField] private float liftingForce  = 10f;
    private bool lift = false;
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
        controller.Player.Crouch.canceled  += Crouch_canceled;

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
        skinColliderCalculate();

        GroundedCalculate();

        SpeedLimiter();


        Debug.DrawRay(transform.position - new Vector3(0, normalHeight * 0.5f - lowerOffset, 0), transform.right * movInpVec.x, Color.red, 0);
        Debug.DrawRay(transform.position - new Vector3(0, normalHeight * 0.5f - lowerOffset, 0), transform.forward * movInpVec.y, Color.blue, 0);
        Debug.DrawRay(transform.position - new Vector3(0, normalHeight * 0.5f - lowerOffset, 0),
            (transform.forward + transform.right) * SignZero(movInpVec.x + movInpVec.y), Color.yellow, 0);
        Debug.DrawRay(transform.position - new Vector3(0, normalHeight * 0.5f - lowerOffset, 0),
            (transform.forward + -transform.right) * SignZero(-movInpVec.x + movInpVec.y), Color.green, 0);


        DragController();

        Crouch();
    }

    private void FixedUpdate()
    {
        if (neck != null)
            neck.localPosition = Vector3.Lerp(neck.localPosition, currCamPos, camToCrouchSpeed * Time.deltaTime);
        else
            Debug.LogWarning("There is no neck on ur player");

        lift = false;
        StepRaycaster(transform.forward * movInpVec.y);
        StepRaycaster(transform.right * movInpVec.x);
        StepRaycaster((transform.forward + transform.right) * SignZero(movInpVec.x + movInpVec.y));
        StepRaycaster((transform.forward + -transform.right) * SignZero(-movInpVec.x + movInpVec.y));
        if (lift)
            rb.AddForce(transform.up * liftingForce, ForceMode.Force);

        if (movementEnabled)
            Movement();
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
        if (holdForCrouch)
            isCrouching = true;
        else if (CanStand())
            isCrouching = !isCrouching;
    }

    private void Crouch_canceled(InputAction.CallbackContext ctx)
    {
        if (holdForCrouch && CanStand())
            isCrouching = false;
    }
    #endregion

    #region Voids
    private void Movement()
    {
        Vector3 moveDir = transform.forward * movInpVec.y + transform.right * movInpVec.x;

        SpeedCalculate();

        rb.AddForce(moveDir * currentSpeed, ForceMode.Acceleration);
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            capsColl.height = crouchHeight;
            capsColl.radius = crouchRadius;
            capsColl.center = new Vector3(0, -0.5f, 0);
            currCamPos = new Vector3(0, crouchCamOffset, 0);
        }
        else
        {
            capsColl.height = normalHeight;
            capsColl.radius = normalRadius;
            capsColl.center = Vector3.zero;
            currCamPos = new Vector3(0, standCamOffset, 0);
        }
    }

    private bool CanStand()
    {
        if (!Physics.Raycast(transform.position, transform.up, 1))
            return true;
        else
            return false;
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

    private void StepRaycaster(Vector3 direction)
    {
        currRadius = capsColl.radius;
        if (Physics.Raycast(transform.position - new Vector3(0, normalHeight * 0.5f - lowerOffset, 0), direction, currRadius + lowerLength, groundMask))
        {
            if (!Physics.Raycast(transform.position - new Vector3(0, normalHeight * 0.5f - higherOffset, 0), direction, currRadius + higherLength, groundMask))
            {
                lift = true; 
            }
        }
    }

    #endregion

    #region Calculators
    private void GroundedCalculate()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, currRadius, Vector3.down, out hit, normalHeight * 0.5f - currRadius + 0.2f, groundMask))
            isGrounded = true;
        else
            isGrounded = false;
    }

    private void skinColliderCalculate()
    {
        if (skinCollider != null)
        {
            skinCollider.center = capsColl.center;
            skinCollider.height = capsColl.height - .05f;
            skinCollider.radius = capsColl.radius + .01f;
        }
        else
            Debug.LogWarning("There is no skinCollider on ur player");
    }

    private void SpeedCalculate()
    {
        if (isGrounded)
        {
            if (movInpVec != new Vector2(0, 0))
            {
                if (isCrouching)
                {
                    currentSpeed = crouchSpeed;
                    camController.currHbState = CameraController.HeadbobStates.crouch;
                }
                else if (isRuning)
                {
                    currentSpeed = runSpeed;
                    camController.currHbState = CameraController.HeadbobStates.run;
                }
                else
                {
                    currentSpeed = walkSpeed;
                    camController.currHbState = CameraController.HeadbobStates.walk;
                }
            }
            else
            {
                currentSpeed = 0;
                camController.currHbState = CameraController.HeadbobStates.idle;
            }
        }
        else
        {
            currentSpeed = airSpeed;
            camController.currHbState = CameraController.HeadbobStates.idle;
        }
    }

    private void SpeedLimiter()
    {
        Vector3 pVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (pVelocity.magnitude > currentSpeed)
        {
            Vector3 limitVelocity = pVelocity.normalized * currentSpeed;
            rb.velocity = new Vector3(limitVelocity.x, rb.velocity.y, limitVelocity.z);
        }
    }

    #endregion

    #region Utilities
    private int SignZero(float x)
    {
        if (x < 0) return -1;
        else if (x > 0) return 1;
        else return 0;
    }
    #endregion
}
