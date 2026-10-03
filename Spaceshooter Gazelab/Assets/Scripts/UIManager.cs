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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text livesText;
    public Text scoreText;

    static UIManager instance;

    int totalScore = 0;
    
    public static int GetCurrentScore()
    {
        if (instance != null)
        {
            return instance.totalScore;
        }
        return 0;
    }
    void Awake()
    {
        instance = this;
        UpdateScoreUI();
    }

    public static void AddScore(int amount)
    {
        if (instance == null) return;
        instance.totalScore += amount;
        instance.UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
    }

    public static void UpdateLives(int lives)
    {
        if (instance != null && instance.livesText != null)
            instance.livesText.text = "Lives: " + lives;
    }
}
