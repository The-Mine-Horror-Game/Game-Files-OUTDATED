using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Camera Things")]
    public Transform orientation;
    public Transform cameraPosition;

    [Header("Crouching")]
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchPosition = new Vector3(0, -0.5f, 0);
    [SerializeField] private Vector3 standPosition = new Vector3(0, 0, 0);
    [SerializeField] private Quaternion playerRotation = new Quaternion();
    public KeyCode crouchKey;

    private bool duringCrouchAnimation = false;
    public bool isCrouching = false;


    private float horizontalInput;
    private float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        MyInput();
        SpeedControl();
        CrouchCheck();

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    private void MyInput()
    {
        // Get horizontal and "vertical" inputs (wasd) for later use
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Add force in that direction
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void CrouchCheck()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            Vector3 temp = new Vector3(cameraPosition.position.x, -playerHeight/2, cameraPosition.position.z);

            cameraPosition.position = Vector3.Lerp(cameraPosition.position, temp, Time.deltaTime * 5);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            Vector3 temp = new Vector3(cameraPosition.position.x, playerHeight/2, cameraPosition.position.z);

            cameraPosition.position = Vector3.Lerp(cameraPosition.position, temp, Time.deltaTime * 5);
        }
    }

    
    private IEnumerator CrouchStand()
    {
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        Vector3 targetPosition = isCrouching ? standPosition : crouchPosition;

        yield return null;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
