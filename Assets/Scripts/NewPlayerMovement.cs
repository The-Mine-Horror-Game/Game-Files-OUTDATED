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

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float gravity = 30.0f;
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;
    private float rotationX = 0;

    void Awake()
    {

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
        if (Input.GetKey(forwardKey))
        {
            currentInput.x = walkSpeed;
        }
        if (Input.GetKey(backKey))
        {
             currentInput.x = -walkSpeed;
        }
        if (Input.GetKey(rightKey))
        {
            currentInput.y = walkSpeed;
        }
        if (Input.GetKey(leftKey))
        {
            currentInput.y = -walkSpeed;
        }

        if ((Input.GetKey(forwardKey) && Input.GetKey(backKey)) )
        {
            currentInput.x = 0;
        }
        if ((Input.GetKey(rightKey) && Input.GetKey(leftKey)) )
        {
            currentInput.y = 0;
        }

        //|| !(Input.GetKey(forwardKey) && Input.GetKey(backKey))
        //|| !(Input.GetKey(rightKey) && Input.GetKey(leftKey))

        

        // What the original code used below, doesn't account for different input keys
        currentInput = new Vector2(walkSpeed * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;

        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {

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
