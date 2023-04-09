using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public bool isInTablet { get; private set; } = false;
    public bool isInMenu { get; private set; } = false;
    private bool IsSprinting => canSprint && playerControls.Player.Sprint.IsInProgress() && stamina > 0 && !staminaRecharging;
    private bool ShouldCrouch => (playerControls.Player.Crouch.WasPressedThisFrame() || playerControls.Player.Crouch.WasReleasedThisFrame()) && !duringCrouchAnimation && characterController.isGrounded || (crouchCancelled && !duringCrouchAnimation);

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
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = 30.0f;
    public bool toggleCrouch = false;

    [Header("Stamina")]
    [SerializeField] private float staminaMax = 5f;
    [SerializeField] private float staminaRegen = 1f;
    [SerializeField] private float stamina;
    [SerializeField] private bool staminaRecharging;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 2f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private bool crouchCancelled;
    [SerializeField] private bool crouchBlocked;
    [SerializeField] private bool crouchQueued;
    [SerializeField] private Transform orientationObj;
    [SerializeField] private bool duringCrouchAnimation;


    [Header("Tablet Parameters")]
    [SerializeField] private bool isCrouching;
    [SerializeField] private GameObject tablet;

    [Header("Menu Parameters")]
    [SerializeField] private GameObject menu;

    public PlayerControls playerControls;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 50)] private float lookSpeedX = 30.0f;
    [SerializeField, Range(1, 50)] private float lookSpeedY = 30.0f;
    [SerializeField, Range(1, 50)] private float leanSens = 30.0f;
    [SerializeField, Range(1, 10)] private float leanSensSlerp = 5.0f;
    [SerializeField, Range(-180, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(-180, 180)] private float lowerLookLimit = -80.0f;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;
    [SerializeField] public bool isLeaning { get; private set; } = false;
    [SerializeField] private Transform playerObjOrientation;
    //[SerializeField] private Transform leanRotationPoint;

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
        stamina = staminaMax;
    }

    void Update()
    {
        HandleTablet();
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            HandleCrouch();
            HandleStamina();
            HandleMenu();

            ApplyFinalMovements();
        }
    }

    private void FixedUpdate()
    {
        //excludes the player from the raycast
        int layerMask = 1 << 6;
        layerMask = ~layerMask;
        crouchBlocked = isCrouching && Physics.Raycast(orientationObj.position, orientationObj.TransformDirection(Vector3.up), 1.5f, layerMask);

        if (crouchBlocked)
        {
            if (playerControls.Player.Crouch.IsPressed())
            {
                crouchQueued = false;
            }
            else
            {
                crouchQueued = true;
            }
        }
    }

    private void HandleMenu()
    {
        if (playerControls.Player.Menu.WasPressedThisFrame() || playerControls.UI.Menu.WasPressedThisFrame() && menu.GetComponent<Menu>().gameStarted)
        {
            isInMenu = !isInMenu;
            menu.SetActive(!menu.activeSelf);
            if(isInMenu)
            {
                playerControls.Player.Disable();
                playerControls.UI.Enable();
            }
            else
            {
                playerControls.UI.Disable();
                playerControls.Player.Enable();
            }
        }
    }

    private void HandleTablet()
    {
        if (playerControls.Player.Tablet.WasPressedThisFrame() || playerControls.Tablet.Tablet.WasPressedThisFrame())
        {
            isInTablet = !isInTablet;
            tablet.SetActive(!tablet.activeSelf);
        }
        if (isInTablet)
        {
            playerControls.Tablet.Enable();
            playerControls.Player.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerControls.Tablet.Disable();
            playerControls.Player.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }

    // Handles keyboard inputs
    private void HandleMovementInput()
    {
        float moveDirectionY = moveDirection.y;
        currentInput = playerControls.Player.Movement.ReadValue<Vector2>();
        
        currentInput.x *= isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        currentInput.y *= isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        

        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
        moveDirection.y = moveDirectionY;
    }

    private void HandleCrouch()
    {
        if ((ShouldCrouch || crouchQueued) && !crouchBlocked)
        {
            crouchQueued = false;
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleStamina()
    {
        if (IsSprinting && currentInput != new Vector2(0, 0))
        {
            stamina -= Time.deltaTime;
        }
        else if (stamina < staminaMax)
        {
            if (stamina <= 0)
            {
                staminaRecharging = true;
            }
            stamina += staminaRegen * Time.deltaTime;
        }
        else if (stamina > staminaMax)
        {
            stamina = staminaMax;
        }
        else
        {
            staminaRecharging = false;
        }
    }

    private void HandleMouseLook()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * lookSpeedX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * lookSpeedY;

        // Slows down mouse movement when in tablet
        if (isInTablet)
        {
            mouseX *= 0.05f;
            mouseY *= 0.05f;
        }
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
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;
        Vector3 cameraCurrentPosition = playerCamera.transform.position;

        Vector3 targetPosition = isCrouching ? new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y + 0.5f, playerCamera.transform.position.z) : new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y - 0.5f, playerCamera.transform.position.z);
        
        while (timeElapsed < timeToCrouch)
        {
            cameraCurrentPosition = new Vector3(transform.position.x, targetPosition.y, transform.position.z);
            //targetPosition = isCrouching ? new Vector3(playerCamera.)

            
            if ((!isCrouching) && (!playerControls.Player.Crouch.IsPressed()))
            {
                currentHeight = characterController.height;
                currentCenter = characterController.center;
                timeElapsed = timeToCrouch - timeElapsed;
                targetHeight = standingHeight;
                targetCenter = standingCenter;
                isCrouching = true;
                crouchCancelled = true;
            }

            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            //playerCamera.transform.position = Vector3.Lerp(cameraCurrentPosition, targetPosition, timeElapsed / timeToCrouch);

            //characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            //characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        isCrouching = !isCrouching;
        crouchCancelled = false;
        
        //playerCamera.transform.position = isCrouching ? new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y + 0.5f, playerCamera.transform.position.z) : new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y - 0.5f, playerCamera.transform.position.z);

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        crouchQueued = false;
        duringCrouchAnimation = false;
    }
}
