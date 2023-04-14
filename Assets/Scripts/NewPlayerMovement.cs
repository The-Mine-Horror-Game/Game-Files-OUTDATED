using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour
{
    /*
     Handles player movement, including crouching and sprinting.
     
     Jacob Hubbard & Kenny Smith
     12/02/23 and onward...

    Comments last updated 12/04/2023 ANY CHANGES PAST THIS POINT MAY RESULT IN INCORRECT COMMENTS. PLEASE KEEP THEM UPDATED REGULARLY.
     */

    // {get; private set; } means that other scripts can fetch this value, but can't alter it.
    public bool CanMove { get; private set; } = true;
    public bool isInTablet { get; private set; } = false;

    // These are conditional bools. Basically, the game will recognize the player as sprinting if all the boolean algebra is fulfilled
    // In this case it's if they can sprint, if they're holding the sprint key, their stamina is above zero and their stamina isn't recharging.
    private bool IsSprinting => canSprint && playerControls.Player.Sprint.IsInProgress() && stamina > 0 && !staminaRecharging;

    // Crouch is a bit of a beast, if you don't understand boolean algebra I suggest you go figure out how to do it then come back here or none of this will make any sense.
    // ShouldCrouch actually affects whether the player should switch crouch positions, i.e. if they're crouching to stand or if they're standing to crouch.
    // More on that later. For now, just trust that it's logic that almost completely stops weird stuff from happening.
    private bool ShouldCrouch => (playerControls.Player.Crouch.WasPressedThisFrame() || playerControls.Player.Crouch.WasReleasedThisFrame()) && !duringCrouchAnimation && characterController.isGrounded || (crouchCancelled && !duringCrouchAnimation);


    //All of these are important, and they all serve a purpose. Read through carefully as I try my best to explain it all


    // If they can sprint, changes during runtime
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;


    // These determine how fast the player moves, all quite self explanatory.
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float gravity = 30.0f;

    // This was a proposed feature to have a toggle crouch, it's still in the air whether we'll implement it. This bool would enable or disable the feature.
    //public bool toggleCrouch = false;


    // The parameters for stamina. Includes max, regen rate, the current stamina, and a bool of if it's recharging.
    [Header("Stamina Parameters")]
    [SerializeField] private float staminaMax = 5f;
    [SerializeField] private float staminaRegen = 1f;
    [SerializeField] private float stamina;
    [SerializeField] private bool staminaRecharging;


    // The parameters for the crouching. Includes the CharacterController standing/crouching height & center variables.
    // Includes whether the crouch is blocked or was cancelled midway through the animation.
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 2f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    [SerializeField] private bool crouchCancelled;
    [SerializeField] private bool crouchBlocked;
    [SerializeField] private bool crouchQueued;
    [SerializeField] private Transform orientationObj; // the green capsule attached to the fps controller to represent the orientation of the player's body.
    [SerializeField] private bool duringCrouchAnimation; // self explanatory


    // Self explanatory
    [Header("Tablet Parameters")]
    [SerializeField] private bool isCrouching;
    [SerializeField] private GameObject tablet;

    // Self explanatory
    [Header("Menu Parameters")]
    [SerializeField] private GameObject menu;
    public bool isInMenu = false;

    // This is less self explanatory. PlayerControls is the input system we have for all the input action maps in the game. It needs to be initialised in void Awake to function properly.
    // Read up on the Unity Input System to learn more.
    public PlayerControls playerControls;


    // Parameters that change how the looking around feels. These are what we think are good numbers.
    [Header("Look Parameters")]
    [SerializeField, Range(1, 50)] private float lookSpeedX = 30.0f;
    [SerializeField, Range(1, 50)] private float lookSpeedY = 30.0f;
    [SerializeField, Range(1, 50)] private float leanSens = 30.0f;
    [SerializeField, Range(1, 10)] private float leanSensSlerp = 5.0f;
    [SerializeField, Range(-180, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(-180, 180)] private float lowerLookLimit = -80.0f;
    [SerializeField] private float xRotation; // The current rotations from the mouse, representing x and y rotation of the player. These are degree values.
    [SerializeField] private float yRotation;
    [SerializeField] public bool isLeaning { get; private set; } = false;
    [SerializeField] private Transform playerObjOrientation; // this is technically unnecessary, but I got tired of writing orientationObj.transform.whatever so I made this.

    private Camera playerCamera;
    private CharacterController characterController; // The characterController of the fps controller.

    // The direction that is plugged into the character controller every frame for 3d movement. No jumping so it's technically 2d but whatever.
    private Vector3 moveDirection;
    // These are the unedited values from the mouse (or will be more like)
    private Vector2 currentInput;

    void Awake()
    {
        // Connect playerControls to the PlayerControls input system.
        playerControls = new PlayerControls();

        // Enables the player action map
        playerControls.Player.Enable();
        // Sets the camera and character controller, easier than dragging them in the inspector
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        // Locks the cursor in the screen and makes it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Sets stamina to max
        stamina = staminaMax;
    }

    void Update()
    {
        // This is the main brains logic of the script, if CanMove is ever false it's the equivalent to disabling the script. Nothing happens.
        // The functions will be described at their location in the script. Use ctrl + f to find them if you have trouble.
        if (CanMove)
        {
            HandleMovementInput();

            // Stops the mouse input from being registered while in the menu.
            if (!isInMenu)
            {
                HandleMouseLook();
            }
            HandleCrouch();
            HandleStamina();
            HandleMenu();

            // Stops you from being able to open the tablet while in the menu.
            if (!isInMenu)
            {
                HandleTablet();
            }
            ApplyFinalMovements();
        }
    }

    // This is solely to check if the player has a collider directly above their head blocking their stand up. FixedUpdate is used for physics stuff.
    private void FixedUpdate()
    {
        //excludes the player from the raycast. It's weird but just trust it's pretty cool that it works.
        int layerMask = 1 << 6;
        layerMask = ~layerMask;
        // Sends the raycast upwards from the orientationObj 1.5m, and returns true if it hits a non-player collider
        crouchBlocked = isCrouching && Physics.Raycast(orientationObj.position, orientationObj.TransformDirection(Vector3.up), 1.5f, layerMask);

        // Logic for queuing the crouch for a future release. Else if you held crouch, went in a vent, let go, and exited nothing would happen.
        if (crouchBlocked)
        {
            // If they're currently holding the key don't mess with it lol.
            if (playerControls.Player.Crouch.IsPressed())
            {
                crouchQueued = false;
            }
            // If they aren't holding the key, we want them to stand back up again so queue the crouch.
            else
            {
                crouchQueued = true;
            }
        }
    }

    // Handles the enabling and disabling of the menu, via either escape or clicking the continue button.
    private void HandleMenu()
    {
        if (playerControls.Player.Menu.WasPressedThisFrame() || playerControls.UI.Menu.WasPressedThisFrame() && menu.GetComponent<Menu>().gameStarted || menu.GetComponent<Menu>().gameUnpaused)
        {
            // flips the state of isInMenu
            isInMenu = !isInMenu;
            // If they're in the menu, disable player action map and enable ui action map, unlock cursor
            if(isInMenu)
            {
                playerControls.Player.Disable();
                playerControls.UI.Enable();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Do the opposite.
            else
            {
                playerControls.UI.Disable();
                playerControls.Player.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            // Turns on and off the menu GameObject.
            menu.SetActive(!menu.activeSelf);
            if (menu.GetComponent<Menu>().gameUnpaused)
            {
                // Resets the gameUnpaused bool from the menu script.
                menu.GetComponent<Menu>().gameUnpaused = false;
            }
        }
    }


    // Basically the exact same as HandleMenu(), except with simpler logic and with the tablet
    private void HandleTablet()
    {
        if (playerControls.Player.Tablet.WasPressedThisFrame() || playerControls.Tablet.Tablet.WasPressedThisFrame())
        {
            isInTablet = !isInTablet;
            tablet.SetActive(!tablet.activeSelf);
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
        
    }

    // Handles keyboard inputs
    private void HandleMovementInput()
    {
        // Creates a temporary float to save the y position of the player. We don't have jumping so useless, but may be useful later?
        float moveDirectionY = moveDirection.y;
        // Converts wasd inputs into a vector2 of the inputs. Very cool!
        currentInput = playerControls.Player.Movement.ReadValue<Vector2>();
        
        // Applies the speed change logic.
        currentInput.x *= isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        currentInput.y *= isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        
        // Sets the place where we will tell the character controller to move to.
        moveDirection = (orientationObj.TransformDirection(Vector3.forward) * currentInput.y) + (orientationObj.TransformDirection(Vector3.right) * currentInput.x);
        // Resetting that pesky y position of the player. Idk man it's just here don't delete it.
        moveDirection.y = moveDirectionY;
    }


    // Nice and simple. If you should crouch and it ain't blocked, change crouch state.
    private void HandleCrouch()
    {
        if ((ShouldCrouch || crouchQueued) && !crouchBlocked)
        {
            crouchQueued = false;
            StartCoroutine(CrouchStand());
        }
    }


    // Handles everything stamina.
    private void HandleStamina()
    {
        // Decreases stamina if you're sprinting and moving
        if (IsSprinting && currentInput != new Vector2(0, 0))
        {
            stamina -= Time.deltaTime;
        }
        // Rest just read lol, the variable names are spot on thanks Kenny!
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


    // Handles everything mouse related. So many hours... oh god...
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


        // Stops the camera from rotating more than the set limit up or down
        xRotation = Mathf.Clamp(xRotation, lowerLookLimit, upperLookLimit);

        // Actually rotates the player camera and orientation. The logic is for leaning, which will likely be removed or revamped before release.
        // If you think I understand what any of this means, I'm sorry to let you down. (I'm only slightly joking)
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
