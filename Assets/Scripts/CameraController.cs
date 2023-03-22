using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("shrek")]
    [SerializeField] private GameObject[] cameraArray;
    [SerializeField] private int cameraCount = 10;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void SwitchCamera()
    {
        Debug.Log("Cam 1 activated");
        return;
    }
}
