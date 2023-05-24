using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{

    [SerializeField] private Transform playerCamera;
    [SerializeField, Range(0.1f, 10f)] private float playerInteractDistance = 1f;
    [SerializeField] public bool playerLookingAtInteractible { get; private set; } = false;

    public PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Update()
    {
        int layerMask = 1 << 7;
        RaycastHit hit;
        playerLookingAtInteractible = Physics.Raycast(playerCamera.position, playerCamera.TransformDirection(Vector3.forward), out hit, playerInteractDistance, layerMask);

        if (playerControls.Player.Interact.WasPressedThisFrame() && playerLookingAtInteractible == true)
        {
             Interact(hit.transform.gameObject); 
        }

        //DEBUG STUFF (draws a line)
        Vector3 interactionRayEndpoint = playerCamera.forward * playerInteractDistance;

        if (playerLookingAtInteractible)
        {
            Debug.DrawLine(playerCamera.position, interactionRayEndpoint);
        }
    }

    public GameObject Interact(GameObject interactible)
    {
        // Do whatever it is we're going to do for an inventory sytem

        //interactible.SetActive(false);

        Debug.Log("The interaction was a success!");
        return null;
    }
}
