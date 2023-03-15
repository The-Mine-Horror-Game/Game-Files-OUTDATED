using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    /*
     Handles player movement, including crouching and sprinting.
     
     Jacob Hubbard
     12/02/23
     */

    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && playerControls.Player.Sprint.IsInProgress();
    private bool ShouldCrouch => playerControls.Player.Crouch.IsInProgress() && !duringCrouchAnimation && characterController.isGrounded || crouchCancelled;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;

    [Header("Controls")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float gravity = 30.0f;
    public bool toggleCrouch = false;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 2f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 crouchingScale = new Vector3(1, 0.25f, 1);
    [SerializeField] private Vector3 standingScale = new Vector3(1, 1, 1);
    [SerializeField] private bool crouchCancelled;

    [SerializeField] private Vector3 currentCameraPosition = new Vector3(0, 0, 0);
    [SerializeField] private float standingCamDelta = 0.5f;
    [SerializeField] private float crouchingCamDelta = -0.5f;

    private bool isCrouching;
    private bool duringCrouchAnimation;

    private PlayerControls playerControls;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 50)] private float lookSpeedX = 30.0f;
    [SerializeField, Range(1, 50)] private float lookSpeedY = 30.0f;
    [SerializeField, Range(1, 50)] private float leanSens = 5.0f;
    [SerializeField, Range(1, 10)] private float leanSensSlerp = 7.0f;
    [SerializeField, Range(-180, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(-180, 180)] private float lowerLookLimit = -80.0f;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;
    [SerializeField] public bool isLeaning { get; private set; } = false;
    [SerializeField] private Transform playerObjOrientation;
    [SerializeField] private Transform leanRotationPoint;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Player.Enable();
        // Sets the camera and character controller, easier than dragging them in the inspector
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        // Locks the cursor in the screen and makes it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            HandleCrouch();

            ApplyFinalMovements();
        }
    }

    // Handles keyboard inputs
    private void HandleMovementInput()
    {
        float moveDirectionY = moveDirection.y;
        
        currentInput = playerControls.Player.Movement.ReadValue<Vector2>();
        currentInput.x *= (IsSprinting ? sprintSpeed : walkSpeed);
        currentInput.y *= (IsSprinting ? sprintSpeed : walkSpeed);

        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
        moveDirection.y = moveDirectionY;
    }

    private void HandleCrouch()
    {
        if (playerControls.Player.Crouch.WasReleasedThisFrame())
        {
            crouchCancelled = true;
        }
        if (ShouldCrouch)
        { 
            if(toggleCrouch)
            {
                StartCoroutine(CrouchStand());
            }
            else if(playerControls.Player.Crouch.WasPressedThisFrame() || crouchCancelled)
            {
                StartCoroutine(CrouchStand());
            }
        }
    }

    private void HandleMouseLook()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * lookSpeedX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * lookSpeedY;

        // Just trust this is how unity works, it's weird but that's how it is
        yRotation += mouseX;

        xRotation -= mouseY;

        // Stops the camera from rotating more than 90 degreed up or down
        xRotation = Mathf.Clamp(xRotation, lowerLookLimit, upperLookLimit);

        // rotate cam and orientation, currently only half functional, doesn't actually move the camera only rotates it
        if (playerControls.Player.Lean.ReadValue<float>() > 0)
        {
            isLeaning = true;
            playerObjOrientation.rotation = Quaternion.Euler(0, yRotation, 0);

            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, leanSens);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);
        }
        else if (playerControls.Player.Lean.ReadValue<float>() < 0)
        {
            isLeaning = true;
            playerObjOrientation.rotation = Quaternion.Euler(0, yRotation, 0);

            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, -leanSens);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);
        }
        else
        {
            isLeaning = false;
            playerObjOrientation.rotation = Quaternion.Euler(0, yRotation, 0);
            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);
        }
    }

    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        currentCameraPosition = playerCamera.transform.position;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float targetCamDelta = isCrouching ? standingCamDelta : crouchingCamDelta;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        Vector3 cameraCurrentPosition = playerCamera.transform.position;



        Vector3 targetPosition = isCrouching ? new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y + 0.5f, playerCamera.transform.position.z) : new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y - 0.5f, playerCamera.transform.position.z);
        
        while (timeElapsed < timeToCrouch)
        {
            if (crouchCancelled)
            {
                targetCenter = standingCenter;
                targetHeight = standingHeight;
                currentCenter = characterController.center;
                currentHeight = characterController.height;
                timeElapsed = 0;
                crouchCancelled = !crouchCancelled;
                Debug.Log("This is working!");
            }
            if (isCrouching)
            {
                //transform.position = new Vector3(transform.position.x, transform.position.y + 0.07f, transform.position.z);
            }

            playerCamera.transform.position = Vector3.Lerp(cameraCurrentPosition, targetPosition, timeElapsed / timeToCrouch);

            //characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            //characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
