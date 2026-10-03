/*
 * Maurice Mattick - 3103312
 *
 * Marian Müller - 3103387
 *
 * Rezaul Hoque - 3077415
 *
 * Fardin Afzalzada - 3082980
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    [Header("Scene to load when pressing Back")]
    public string menuSceneName = "MainMenu";

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}