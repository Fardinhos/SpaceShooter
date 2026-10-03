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

public class BossIntroManager : MonoBehaviour
{
    public static BossIntroManager Instance { get; private set; }

    [Header("Boss Setup")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public float introDelay = 3f;

    [Header("Spawning")]
    public bool stopSpawningOnIntro = true;

    public bool BossSequenceStarted { get; private set; }
    public bool BossSpawned { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartBossSequence()
    {
        if (BossSequenceStarted)
            return;

        BossSequenceStarted = true;

        if (stopSpawningOnIntro)
        {
            SpawnManager.GlobalSpawningEnabled = false;
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBossMusic();
        }

        StartCoroutine(BossSequenceRoutine());
    }

    private IEnumerator BossSequenceRoutine()
    {
        yield return new WaitForSeconds(introDelay);

        if (!BossSpawned && bossPrefab != null && bossSpawnPoint != null)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            BossSpawned = true;
        }
    }
}
