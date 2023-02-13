using UnityEngine;
using TMPro;

/*
*   Records the user's last input as a keycode.
*   This program is to be changed/adapted to whatever
*   menu we end up using. This program assumes that 
*   the menu already pauses user inputs for the game.
*   The commented update function is much less efficient
*   than the OnGUI function, however it doesn't require
*   the mouse buttons to be hard coded in. We can decide 
*   which one to use based off of how it impacts the game's
*   actual performance.
*
*   Kenny Smith
*   12/02/23
*/

public class KeyDetection : MonoBehaviour
{

    // whenever a gui event is being processed (keyboard press, mouse click, etc)
    void OnGUI()
    {
        // if the event is a key first being pushed down and it has a keycode
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None)
        {
            // set a temporary text object to the keycode (for testing,
            // will be populating a var later)
            GetComponent<TextMeshPro>().text = "" + Event.current.keyCode;
        }
        // else if the event being processed is mouse related
        else if (Event.current.type == EventType.MouseDown)
        {
            // since event.button returns an int (0 for left click, 1 for right click, etc)
            // we have to hard code mouse button keycodes into a switch statement to return
            // the proper keycode values. this is still miles more efficient than the commented
            // code above that checks every frame whether or not every key was pressed
            switch (Event.current.button){
                case 0:
                    GetComponent<TextMeshPro>().text = "" + KeyCode.Mouse0;
                    break;
                case 1:
                    GetComponent<TextMeshPro>().text = "" + KeyCode.Mouse1;
                    break;
                case 2:
                    GetComponent<TextMeshPro>().text = "" + KeyCode.Mouse2;
                    break;
                case 3:
                    GetComponent<TextMeshPro>().text = "" + KeyCode.Mouse3;
                    break;
                case 4:
                    GetComponent<TextMeshPro>().text = "" + KeyCode.Mouse4;
                    break;
            }
        }
    }

    /*
    // called every frame
    void Update()
    {

        // loops through every possible keycode value (328)
        foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {

            // if the user has just hit the key
            if (Input.GetKeyDown(key))
            {

                // set tmp to the key (temp code, to be replaced with populating var)
                GetComponent<TextMeshPro>().text = "" + key;

            }
        }

    }
    */
}
