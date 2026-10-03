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
using UnityEngine.UI;
using System.Collections;

public class WonMenu : MonoBehaviour
{
    public Text promptText;
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        Time.timeScale = 1f;
        Input.ResetInputAxes();
        if (promptText) StartCoroutine(Blink());
    }

    void Update()
    {
        if (ConfirmDown()) StartCoroutine(ReturnToMenu());
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    IEnumerator ReturnToMenu()
    {
        while (ConfirmHeld()) yield return null;
        yield return null;
        Input.ResetInputAxes();
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    IEnumerator Blink()
    {
        while (true)
        {
            promptText.enabled = !promptText.enabled;
            yield return new WaitForSecondsRealtime(0.6f);
        }
    }

    bool ConfirmDown() =>
        Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    bool ConfirmHeld() =>
        Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter);
}
