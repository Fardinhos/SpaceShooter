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
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    public Text[] items; 
    public RectTransform arrow;   
    public Color normalColor = Color.gray;
    public Color selectedColor = Color.white;

    [Header("Input")]
    public float acceptDelay = 0.35f;
    public bool wrap = true;

    [Header("Aktionen")]
    public UnityEvent[] onSubmit;

    int index;
    float acceptAt;

    void Start()
    {
        Time.timeScale = 1f;
        Input.ResetInputAxes();
        acceptAt = Time.unscaledTime + acceptDelay;
        index = Mathf.Clamp(index, 0, items.Length - 1);
        UpdateVisuals();
    }

    void Update()
    {
        if (Time.unscaledTime < acceptAt || items == null || items.Length == 0) return;

        if (DownPressed()) Move(+1);
        if (UpPressed()) Move(-1);

        if (ConfirmPressed())
        {
            if (onSubmit != null && index < onSubmit.Length && onSubmit[index] != null)
                onSubmit[index].Invoke();
        }
    }

    void Move(int dir)
    {
        int n = items.Length, next = index + dir;
        next = wrap ? (next % n + n) % n : Mathf.Clamp(next, 0, n - 1);
        if (next != index) { index = next; UpdateVisuals(); }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < items.Length; i++)
            if (items[i]) items[i].color = (i == index) ? selectedColor : normalColor;

        if (arrow && items[index])
        {
            var t = items[index].rectTransform;
            arrow.position = new Vector3(arrow.position.x, t.position.y, arrow.position.z);
        }
    }

    bool DownPressed() => Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    bool UpPressed() => Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
    bool ConfirmPressed() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);

    public void LoadSceneByName(string scene) => SceneManager.LoadScene(scene);
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}