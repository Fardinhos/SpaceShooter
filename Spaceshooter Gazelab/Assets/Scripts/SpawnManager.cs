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
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static bool GlobalSpawningEnabled = true;

    [Header("Normale Gegner")]
    [SerializeField] private GameObject _enemyPrefabA;
    [SerializeField] private GameObject _enemyPrefabB;

    [Tooltip("Wahrscheinlichkeit (0–1), dass Variante A gespawnt wird. Rest ist Variante B.")]
    [Range(0f, 1f)]
    [SerializeField] private float _chanceUseVariantA = 0.5f;

    [Header("Spawn-Intervalle je Schwierigkeit")]
    [Tooltip("Sekunden zwischen Spawns im EASY-Modus.")]
    [SerializeField] private float _spawnIntervalEasy = 5.0f;

    [Tooltip("Sekunden zwischen Spawns im HARD-Modus.")]
    [SerializeField] private float _spawnIntervalHard = 2.5f;

    private float _spawnInterval = 5.0f;

    [Header("Boss-Spawning (Punkte-Schwelle je Schwierigkeit)")]
    [Tooltip("Punkte, die für den Boss im EASY-Modus benötigt werden.")]
    [SerializeField] private int _scoreToSpawnBossEasy = 1000;

    [Tooltip("Punkte, die für den Boss im HARD-Modus benötigt werden.")]
    [SerializeField] private int _scoreToSpawnBossHard = 1500;

    private int _scoreToSpawnBoss = 1000;

    [SerializeField] private GameObject _bossPrefab;

    [Tooltip("Zeit bis der Boss wirklich gespawnt wird (Sekunden).")]
    [SerializeField] private float _bossSpawnDelay = 5.0f;

    [Tooltip("Zeit bis der Spieler automatisch zurück zur Startposition fliegt (Sekunden).")]
    [SerializeField] private float _playerMoveBackDelay = 2.5f;

    private bool _bossHasSpawned = false;
    private bool _stopSpawning = false;

    private void Awake()
    {
        GlobalSpawningEnabled = true;
        _bossHasSpawned = false;
        _stopSpawning = false;
    }

    private void OnValidate()
    {
        if (_bossSpawnDelay < 0f) _bossSpawnDelay = 0f;
        if (_playerMoveBackDelay < 0f) _playerMoveBackDelay = 0f;
        if (_playerMoveBackDelay > _bossSpawnDelay)
            _playerMoveBackDelay = _bossSpawnDelay;

        if (_spawnIntervalEasy < 0.1f) _spawnIntervalEasy = 0.1f;
        if (_spawnIntervalHard < 0.1f) _spawnIntervalHard = 0.1f;

        if (_scoreToSpawnBossEasy < 0) _scoreToSpawnBossEasy = 0;
        if (_scoreToSpawnBossHard < 0) _scoreToSpawnBossHard = 0;
    }

    private void Start()
    {
        ApplyDifficultySettings();
        StartCoroutine(SpawnEnemyRoutine());
    }

    private void ApplyDifficultySettings()
    {
        var diff = DifficultyManager.GetSavedDifficulty();

        if (diff == DifficultyManager.Difficulty.Hard)
        {
            _spawnInterval = _spawnIntervalHard;
            _scoreToSpawnBoss = _scoreToSpawnBossHard;
        }
        else
        {
            _spawnInterval = _spawnIntervalEasy;
            _scoreToSpawnBoss = _scoreToSpawnBossEasy;
        }

        Debug.Log($"[SpawnManager] Difficulty={diff}, spawnInterval={_spawnInterval}, scoreToBoss={_scoreToSpawnBoss}");
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (_stopSpawning == false)
        {
            if (UIManager.GetCurrentScore() >= _scoreToSpawnBoss && _bossHasSpawned == false)
            {
                SetGlobalSpawning(false);

                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.PlayBossMusic();
                }

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var bossHandler = player.GetComponent<PlayerBossPhaseHandler>();
                    if (bossHandler != null)
                    {
                        bossHandler.StartBossIntroMovement(_playerMoveBackDelay);
                    }
                }

                yield return new WaitForSeconds(_bossSpawnDelay);

                SpawnBoss();

                _stopSpawning = true;
                yield break;
            }

            if (GlobalSpawningEnabled)
            {
                GameObject chosenPrefab = ChooseEnemyVariant();

                if (chosenPrefab != null)
                {
                    float randomY = Random.Range(-4.5f, 4.5f);
                    Vector3 spawnPosition = new Vector3(11f, randomY, 0);
                    Instantiate(chosenPrefab, spawnPosition, chosenPrefab.transform.rotation);
                }
            }

            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private GameObject ChooseEnemyVariant()
    {
        if (_enemyPrefabA != null && _enemyPrefabB == null)
            return _enemyPrefabA;

        if (_enemyPrefabB != null && _enemyPrefabA == null)
            return _enemyPrefabB;

        if (_enemyPrefabA == null && _enemyPrefabB == null)
        {
            Debug.LogWarning("[SpawnManager] Kein Enemy-Prefab zugewiesen!");
            return null;
        }

        float r = Random.value;
        return (r <= _chanceUseVariantA) ? _enemyPrefabA : _enemyPrefabB;
    }

    private void SpawnBoss()
    {
        _bossHasSpawned = true;

        if (_bossPrefab == null)
        {
            Debug.LogError("[SpawnManager] Boss Prefab fehlt!");
            return;
        }

        Vector3 spawnPos = new Vector3(11f, 0f, 0f);
        Instantiate(_bossPrefab, spawnPos, _bossPrefab.transform.rotation);

        Debug.Log("[SpawnManager] Boss gespawnt.");
    }

    public static void SetGlobalSpawning(bool enabled)
    {
        GlobalSpawningEnabled = enabled;
        Debug.Log("[SpawnManager] Global spawning = " + enabled);
    }

    public void OnPlayerDied()
    {
        Debug.Log("[SpawnManager] OnPlayerDied aufgerufen – Szene wird neu geladen.");

        _stopSpawning = false;
        _bossHasSpawned = false;
        GlobalSpawningEnabled = true;

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
