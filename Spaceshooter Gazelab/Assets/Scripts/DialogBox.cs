/*
 * Maurice Mattick - 3103312
 *
 * Marian Müller - 3103387
 *
 * Rezaul Hoque - 3077415
 *
 * Fardin Afzalzada - 3082980
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox : MonoBehaviour
{
    [Header("UI (uGUI)")]
    public GameObject panel;
    public Text text;
    public Graphic[] nextIcons;

    [Header("Effekte")]
    public float charsPerSecond = 40f;
    public AudioSource blip;

    string[] pages;
    int pageIndex;
    bool showing, pageDone;

    void Awake() => HideImmediate();

    public IEnumerator Show(string[] lines)
    {
        pages = lines; pageIndex = 0;
        Time.timeScale = 0f;
        panel.SetActive(true);
        showing = true;

        while (ConfirmHeld()) yield return null;
        yield return null;
        Input.ResetInputAxes();

        while (showing)
        {
            yield return StartCoroutine(TypePage(pages[pageIndex]));

            while (!ConfirmDown())
            {
                BlinkNextIcons();
                yield return null;
            }
            SetIconsActive(false);

            while (ConfirmHeld()) yield return null;
            yield return null;
            Input.ResetInputAxes();

            if (pageIndex < pages.Length - 1) pageIndex++;
            else Close();
        }
    }

    IEnumerator TypePage(string s)
    {
        pageDone = false;
        text.text = "";
        float t = 0f;
        int i = 0;

        while (i < s.Length)
        {
            if (ConfirmDown()) { text.text = s; break; }

            t += Time.unscaledDeltaTime * charsPerSecond;
            int nextCount = Mathf.FloorToInt(t);
            for (; i < nextCount && i < s.Length; i++)
            {
                text.text += s[i];
                if (blip) blip.Play();
            }
            yield return null;
        }
        pageDone = true;
    }

    void Close()
    {
        HideImmediate();
        Time.timeScale = 1f;
        showing = false;
    }

    void HideImmediate()
    {
        if (panel) panel.SetActive(false);
        SetIconsActive(false);
        if (text) text.text = "";
    }

    void BlinkNextIcons()
    {
        if (!pageDone) { SetIconsActive(false); return; }
        bool show = (Mathf.FloorToInt(Time.unscaledTime * 2f) % 2) == 0;
        SetIconsActive(show);
    }

    void SetIconsActive(bool on)
    {
        if (nextIcons == null) return;
        for (int i = 0; i < nextIcons.Length; i++)
            if (nextIcons[i]) nextIcons[i].gameObject.SetActive(on);
    }

    bool ConfirmDown() =>
        Input.GetKeyDown(KeyCode.Return) ||
        Input.GetKeyDown(KeyCode.KeypadEnter);

    bool ConfirmHeld() =>
        Input.GetKey(KeyCode.Return) ||
        Input.GetKey(KeyCode.KeypadEnter);
}