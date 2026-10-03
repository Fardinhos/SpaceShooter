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

public class PlatformSpawner : MonoBehaviour
{
    [Header("Was wird gespawnt?")]
    public GameObject[] platformPrefabs;
    public GameObject platformPrefab;

    [Header("Spawn-Einstellungen")]
    public float spawnEvery = 5.0f;
    public float spawnInset = 0.5f;
    public float spawnOffsetX = 1.0f;

    [Header("Bewegung")]
    public float speed = 3f;
    public Vector2 direction = Vector2.left;

    [Header("AufrÃ¤umen")]
    public float despawnMargin = 1.0f;

    Rect camRect;
    float rightSpawnX;

    void Start()
    {
        var cam = Camera.main;
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        camRect = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
        rightSpawnX = tr.x + spawnOffsetX;

        if (SpawnManager.GlobalSpawningEnabled)
            Spawn();

        InvokeRepeating(nameof(Spawn), spawnEvery, spawnEvery);
    }

    GameObject PickPlatformPrefab()
    {
        if (platformPrefabs != null && platformPrefabs.Length > 0)
            return platformPrefabs[Random.Range(0, platformPrefabs.Length)];

        if (platformPrefab != null)
            return platformPrefab;

        Debug.LogError("[PlatformSpawner] Kein Platform-Prefab zugewiesen!");
        return null;
    }

    void Spawn()
    {
        if (!SpawnManager.GlobalSpawningEnabled)
            return;

        float y = Random.Range(camRect.yMin + spawnInset, camRect.yMax - spawnInset);

        var prefab = PickPlatformPrefab();
        if (prefab == null) return;

        Vector3 pos = new Vector3(rightSpawnX, y, 0f);
        var go = Instantiate(prefab, pos, Quaternion.identity);
        var mover = go.GetComponent<LifeMover2D>() ?? go.AddComponent<LifeMover2D>();
        mover.speed = speed * Random.Range(0.9f, 1.15f);
        mover.direction = direction;
        mover.SetBounds(camRect, despawnMargin);
    }
}
