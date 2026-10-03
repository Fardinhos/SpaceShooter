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

public class AsteroidSpawner : MonoBehaviour
{
    [Header("Was wird gespawnt?")]
    public GameObject[] asteroidPrefabs;
    public GameObject asteroidPrefab;

    [Header("Spawn-Einstellungen (Basis je Difficulty)")]
    [Tooltip("Sekunden zwischen Spawns im EASY-Modus.")]
    public float spawnEveryEasy = 0.8f;
    [Tooltip("Sekunden zwischen Spawns im HARD-Modus.")]
    public float spawnEveryHard = 0.4f;

    [Tooltip("Wie weit vom oberen/unteren Rand weg gespawnt wird.")]
    public float spawnInset = 0.5f;
    [Tooltip("Wie weit rechts außerhalb des Screens gespawnt wird.")]
    public float spawnOffsetX = 1.0f;

    [Header("Bewegung (Basis je Difficulty)")]
    [Tooltip("Basisspeed der Asteroiden im EASY-Modus.")]
    public float speedEasy = 3f;
    [Tooltip("Basisspeed der Asteroiden im HARD-Modus.")]
    public float speedHard = 4.5f;

    public Vector2 direction = Vector2.left;

    [Header("Aufräumen")]
    public float despawnMargin = 1.0f;

    private float _spawnEvery = 0.5f;
    private float _baseSpeed = 3f;

    Rect camRect;
    float rightSpawnX;

    void Start()
    {
        ApplyDifficultySettings();

        var cam = Camera.main;
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        camRect = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);

        rightSpawnX = tr.x + spawnOffsetX;

        if (SpawnManager.GlobalSpawningEnabled)
            Spawn();

        InvokeRepeating(nameof(Spawn), _spawnEvery, _spawnEvery);
    }

    void ApplyDifficultySettings()
    {
        var diff = DifficultyManager.GetSavedDifficulty();

        if (diff == DifficultyManager.Difficulty.Hard)
        {
            _spawnEvery = Mathf.Max(0.1f, spawnEveryHard);
            _baseSpeed  = Mathf.Max(0.1f, speedHard);
        }
        else
        {
            _spawnEvery = Mathf.Max(0.1f, spawnEveryEasy);
            _baseSpeed  = Mathf.Max(0.1f, speedEasy);
        }

        Debug.Log($"[AsteroidSpawner] Difficulty={diff}, spawnEvery={_spawnEvery}, baseSpeed={_baseSpeed}");
    }

    GameObject PickAsteroidPrefab()
    {
        if (asteroidPrefabs != null && asteroidPrefabs.Length > 0)
            return asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];

        if (asteroidPrefab != null)
            return asteroidPrefab;

        Debug.LogError("[AsteroidSpawner] Kein Asteroiden-Prefab zugewiesen!");
        return null;
    }

    void Spawn()
    {
        if (!SpawnManager.GlobalSpawningEnabled)
            return;

        float y = Random.Range(camRect.yMin + spawnInset, camRect.yMax - spawnInset);

        var prefab = PickAsteroidPrefab();
        if (prefab == null) return;

        Vector3 pos = new Vector3(rightSpawnX, y, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        float s = Random.Range(0.85f, 1.2f);
        go.transform.localScale *= s;

        var mover = go.GetComponent<AsteroidMover2D>() ?? go.AddComponent<AsteroidMover2D>();
        mover.speed = _baseSpeed * Random.Range(0.9f, 1.15f);
        mover.direction = direction;
        mover.SetBounds(camRect, despawnMargin);
    }
}
