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
using System.Collections;

public class TutorialIntro : MonoBehaviour
{
    public DialogBox dialog;
    public string[] lines = {
    };

    IEnumerator Start()
    {
        if (!dialog) dialog = FindObjectOfType<DialogBox>();
        if (!dialog) { Debug.LogError("DialogBox nicht gefunden."); yield break; }

        yield return dialog.Show(lines);
    }
}
