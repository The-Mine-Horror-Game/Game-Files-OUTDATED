using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX;
    public float sensY;
    public float sensLean;
    public float slerpSensLean;
    public bool isLeaning;

    public Transform orientation;
    public Transform cameraPivotTransform;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);



        // rotate cam and orientation
        if (Input.GetKey(KeyCode.Q))
        {
            isLeaning = true;
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, sensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, transform.localRotation.z + sensLean);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, sensLean);
            transform.rotation = Quaternion.Slerp(transform.localRotation, newRot, Time.deltaTime * slerpSensLean);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            isLeaning = true;
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, - sensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, transform.localRotation.z - sensLean);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, -sensLean);
            transform.rotation = Quaternion.Slerp(transform.localRotation, newRot, Time.deltaTime * slerpSensLean);
        }
        else
        {
            isLeaning = false;
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            Quaternion newRot = Quaternion.Euler(xRotation, yRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.localRotation, newRot, Time.deltaTime * slerpSensLean);


            //transform.position += Vector3.right * sensXDistance;
            //transform.position += Vector3.up * sensYDistance;

            //cameraTransform.position = Vector3.MoveTowards(transform.position, leanRightLocation.position, Time.deltaTime * slerpSensLean);
            //transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}
