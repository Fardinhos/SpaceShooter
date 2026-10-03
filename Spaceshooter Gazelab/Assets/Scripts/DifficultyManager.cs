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

public class DifficultyManager : MonoBehaviour
{
    [Header("Global Difficulty")]
    [Tooltip("Aktuell eingestellte Schwierigkeit.")]
    public Difficulty currentDifficulty = Difficulty.Easy;

    public enum Difficulty
    {
        Easy = 0,
        Hard = 1
    }

    private const string DifficultyKey = "Difficulty";

    private void Awake()
    {
        int stored = PlayerPrefs.GetInt(DifficultyKey, 0);
        if (stored < 0 || stored > 1) stored = 0;

        currentDifficulty = (Difficulty)stored;
        ApplyDifficulty();
    }

    private void OnValidate()
    {
        ApplyDifficulty();
    }

    public void SetEasy()
    {
        currentDifficulty = Difficulty.Easy;
        ApplyDifficulty();
    }

    public void SetHard()
    {
        currentDifficulty = Difficulty.Hard;
        ApplyDifficulty();
    }

    private void ApplyDifficulty()
    {
        PlayerPrefs.SetInt(DifficultyKey, (int)currentDifficulty);
        PlayerPrefs.Save();

        Debug.Log("[DifficultyManager] Difficulty set to " + currentDifficulty);
    }

    public static Difficulty GetSavedDifficulty()
    {
        int stored = PlayerPrefs.GetInt(DifficultyKey, 0);
        if (stored < 0 || stored > 1) stored = 0;
        return (Difficulty)stored;
    }
}
