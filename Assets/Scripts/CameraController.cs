using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject[] cameraArray;
    [SerializeField] private int currentCamera = 0;
    [SerializeField] private int previousCamera = 0;
    [SerializeField] private int tiltAngle = 90;
    [SerializeField] private float smooth = 0.2f;
    [SerializeField] private Vector2 currentInput;
    [SerializeField] private GameObject movementObj;
    [SerializeField] private Transform previousRotation;
    [SerializeField] private PlayerControls camPlayerControls;

    private void Start()
    {
        camPlayerControls = movementObj.GetComponent<NewPlayerMovement>().playerControls;
        previousRotation.rotation = cameraArray[currentCamera].transform.rotation;
    }

    private void Update()
    {
        if (movementObj.GetComponent<NewPlayerMovement>().isInTablet)
        {
            // Smoothly tilts a transform towards a target rotation.
            currentInput = camPlayerControls.Tablet.RotateCam.ReadValue<Vector2>();

            currentInput.x *= tiltAngle;
            currentInput.y *= tiltAngle;

            // Rotate the cube by converting the angles into a quaternion.
            Quaternion target = Quaternion.Euler(-currentInput.y, currentInput.x, 0);

            // Dampen towards the target rotation
            cameraArray[currentCamera].transform.localRotation = Quaternion.Slerp(cameraArray[currentCamera].transform.localRotation, target, Time.deltaTime * smooth);
            cameraArray[currentCamera].transform.localRotation = Quaternion.Euler(cameraArray[currentCamera].transform.rotation.eulerAngles.x, cameraArray[currentCamera].transform.localRotation.eulerAngles.y, 0.0f);
        }
    }
    public void SwitchCamera(int camera)
    {
        camera--;
        currentCamera = camera;

        cameraArray[previousCamera].transform.rotation = previousRotation.rotation;

        cameraArray[previousCamera].SetActive(false);
        cameraArray[currentCamera].SetActive(true);

        previousRotation.rotation = cameraArray[currentCamera].transform.rotation;

        previousCamera = currentCamera;
        return;
    }
}
