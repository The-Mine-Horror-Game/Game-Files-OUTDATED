using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("Menu Parameters")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject quit;
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject continueYEE;
    [SerializeField] private GameObject menuCam;
    [SerializeField] private GameObject quitConfirmation;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField] private GameObject back;
    [SerializeField] private GameObject firstPersonController;
    public PlayerControls playerControls;
    [SerializeField] public bool gameStarted;
    [SerializeField] public bool gameUnpaused;


    private void Start()
    {
        //playerControls = firstPersonController.GetComponent<NewPlayerMovement>().playerControls;
        playerControls = new PlayerControls();
        //playerControls.Player.Disable();
        //playerControls.UI.Enable();
        menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void StartGame()
    {
        start.SetActive(false);
        continueYEE.SetActive(true);
        gameStarted = true;
        firstPersonController.SetActive(true);
        menuCam.SetActive(false);
        menu.SetActive(false);
    }

    public void Back()
    {
        if (quitConfirmation.activeSelf)
        {
            QuitCancel();
        }
        else if (settingsButton.activeSelf)
        {
            CloseSettings();
        }
    }

    public void Continue()
    {
        gameUnpaused = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void QuitCheck()
    {
        quitConfirmation.SetActive(true);
        settingsButton.SetActive(false);
        back.SetActive(false);
        quit.SetActive(false);
        start.SetActive(false);
        continueYEE.SetActive(false);
        return;
    }
    public void QuitCancel()
    {
        quitConfirmation.SetActive(false);
        settingsButton.SetActive(true);
        back.SetActive(true);
        quit.SetActive(true);
        return;
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }

    public void QuitConfirmed()
    {
        Application.Quit();
        return;
    }
}
