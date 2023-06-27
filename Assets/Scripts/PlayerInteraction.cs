using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{

    [SerializeField] private Transform playerCamera;
    [SerializeField, Range(0.1f, 50f)] private float playerInteractDistance = 1f;
    [SerializeField] public bool playerLookingAtInteractible { get; private set; } = false;
    [SerializeField] private GameObject crosshair;
    private GameObject objectInteracted;

    public PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Player.Enable();
    }

    private void FixedUpdate()
    {
        // Masks everything except for the interactible mask
        int layerMask = 1 << 7;
        // Defines a new ray 'hit'
        RaycastHit hit;
        playerLookingAtInteractible = Physics.Raycast(playerCamera.position, playerCamera.TransformDirection(Vector3.forward), out hit, playerInteractDistance, layerMask);

        if (playerLookingAtInteractible)
        {
            objectInteracted = hit.transform.gameObject;
        }
        if (playerControls.Player.Interact.WasPressedThisFrame() && playerLookingAtInteractible == true)
        {
            Interact(hit.transform.gameObject);
        }
    }
    private void Update()
    {
        HandleCrosshair();
        if (playerControls.Player.Interact.WasPressedThisFrame() && playerLookingAtInteractible == true)
        {
            Interact(objectInteracted);
        }
    }

    // Interact with the game object
    public GameObject Interact(GameObject interactible)
    {
        // Do whatever it is we're going to do for an inventory sytem

        //Turns off whatever you're interacting with
        interactible.SetActive(false);

        Debug.Log("The interaction was a success!");
        return null;
    }

    public void HandleCrosshair()
    {
        // Fades in the crosshair when hovering over an interactible object
        if (playerLookingAtInteractible)
        {
            FadeIn(crosshair.GetComponent<RawImage>());
        }
        // Or fades out when not hovering over an interactible object
        else
        {
            FadeOut(crosshair.GetComponent<RawImage>());
        }
        return;
    }

    // Fades in a raw image in 0.5 seconds
    public void FadeIn(RawImage x)
    {
        x.CrossFadeAlpha(1, 0.5f, false);
        return;
    }
    // Fades out a raw image in 0.1 seconds
    public void FadeOut(RawImage x)
    {
        x.CrossFadeAlpha(0, 0.1f, false);
        return;
    }
}
