using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("Menu Parameters")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject quit;
    [SerializeField] private GameObject quitConfirmation;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject back;
    [SerializeField] private GameObject movementObj;
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] public bool gameStarted;

    private void Start()
    {
        playerControls = movementObj.GetComponent<NewPlayerMovement>().playerControls;
        playerControls.Player.Disable();
        playerControls.UI.Enable();
        menu.SetActive(true);
    }
    public void StartGame()
    {
        playerControls.Player.Enable();
        playerControls.UI.Disable();
        menu.SetActive(false);
        gameStarted = true;
    }
    public void QuitCheck()
    {
        quitConfirmation.SetActive(true);
        settings.SetActive(false);
        back.SetActive(false);
        quit.SetActive(false);
        return;
    }
    public void QuitCancel()
    {
        quitConfirmation.SetActive(false);
        settings.SetActive(true);
        back.SetActive(true);
        quit.SetActive(true);
        return;
    }
    public void QuitConfirmed()
    {
        Application.Quit();
        return;
    }
}
