using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    public Transform cameraPosition;

    void Update()
    {
        // Keeps the camera glued to the player
        transform.position = cameraPosition.position;
    }
}
