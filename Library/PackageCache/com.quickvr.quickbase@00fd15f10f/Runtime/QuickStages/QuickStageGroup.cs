using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Function to load an additive scene
    public void LoadAdditiveScene(string sceneName)
    {
        // Check if the scene is already loaded to avoid reloading it
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }

    // Function to unload a scene
    public void UnloadAdditiveScene(string sceneName)
    {
        // Unload the scene
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}

public class SceneController : MonoBehaviour
{
    // Reference to SceneSwitcher
    public SceneSwitcher sceneSwitcher;

    // Update is called once per frame
    void Update()
    {
        // Check if the "D" key is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Load SceneA additively when the D key is pressed
            sceneSwitcher.LoadAdditiveScene("Forest");
        }

        // Check if the "F" key is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Load SceneB additively when the F key is pressed
            sceneSwitcher.LoadAdditiveScene("Mountain");
        }
    }
}
