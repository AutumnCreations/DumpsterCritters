using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public MusicPlayer musicPlayer; // Reference to the MusicPlayer

    public void LoadScene(int sceneIndex)
    {
        if (musicPlayer != null)
        {
            musicPlayer.StopMusic(); // Stop the music before loading the scene
        }
        SceneManager.LoadScene(sceneIndex);
    }
}

