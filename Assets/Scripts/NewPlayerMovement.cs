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
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);


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
    
    private PlayerControls playerControls;
    private PlayerInput playerInput;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 50)] private float lookSpeedX = 30.0f;
    [SerializeField, Range(1, 50)] private float lookSpeedY = 30.0f;
    [SerializeField, Range(1, 30)] private float leanSens = 5.0f;
    [SerializeField, Range(1, 10)] private float leanSensSlerp = 7.0f;
    [SerializeField, Range(-180, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(-180, 180)] private float lowerLookLimit = -80.0f;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;
    [SerializeField] public bool isLeaning { get; private set; } = false;
    [SerializeField] private Transform orientation;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Player.Enable();

        playerInput = GetComponent<PlayerInput>();
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
        if (Input.GetKey(KeyCode.Q))
        {
            isLeaning = true;
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, sensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, transform.localRotation.z + sensLean);
            //orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, leanSens);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            isLeaning = true;
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, - sensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, transform.localRotation.z - sensLean);
            //orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, -leanSens);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);
        }
        else
        {
            isLeaning = false;
            //orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * leanSensSlerp);

            //transform.position += Vector3.right * lookSpeedXDistance;
            //transform.position += Vector3.up * lookSpeedYDistance;

            //cameraTransform.position = Vector3.MoveTowards(transform.position, leanRightLocation.position, Time.deltaTime * slerpSensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
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
}
