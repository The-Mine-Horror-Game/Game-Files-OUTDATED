using UnityEngine;
using UnityEngine.SceneManagement;

/*
*   Switches the scene when the player touches a certain object.
*   It's also important to not that you have to add the scenes
*   being used to the build settings for it to work.
*
*   Kenny Smith
*   10/02/23
*/

public class SceneSwitcher : MonoBehaviour
{

    [Header("Reference")]
    public Object sceneToLoad;  // holds the scene that the box would load
    public Object player;       // holds the player


    // when something collides with the box
    private void OnTriggerEnter(Collider other)
    {

        // if its the player, load the next scene and doesnt remove the player
        if (other.tag == "Player") 
        {
            SceneManager.LoadScene(sceneToLoad.name, LoadSceneMode.Single); 
            Object.DontDestroyOnLoad(player);
        }
        
    }
}
