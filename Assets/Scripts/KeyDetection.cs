using UnityEngine;
using TMPro;

/*
*   A brute-force input checking method to be adapted
*   and used for editing keybinds in whatever menu is to be made.
*   It's pretty inefficient, but was the only solution I could find
*   and will only be running in the menu while nothing else is happening,
*   so it's probably alright. This program assumes that the menu already 
*   pauses user inputs for the game.
*
*   Kenny Smith
*   12/02/23
*/

public class NewBehaviourScript : MonoBehaviour
{

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
}
