using UnityEngine;

/*
*   Toggles your flashlight when a certain key is pressed
*   Room to add batteries/other limitations
*
*   Kenny Smith
*   10/02/23
*/

public class Flashlight : MonoBehaviour
{

    // decl vars
    public KeyCode flashlightKey;   // holds key that toggles flashlight
    private Light lightComponent;   // holds the light component for quick reference

    // called when obj is created
    void Start() 
    {

        // populate quick reference var
        lightComponent = GetComponent<Light>();

    }

    // called every frame
    void Update()
    {
        // when the flashlight key is initially pushed down
        if (Input.GetKeyDown(flashlightKey))
        {
            // toggle flashlight
            lightComponent.enabled = !lightComponent.enabled;
        }
    }
}
