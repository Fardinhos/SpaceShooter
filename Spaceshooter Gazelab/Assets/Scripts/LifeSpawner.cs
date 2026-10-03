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

public class LifeSpawner : MonoBehaviour
{
    [Header("Was wird gespawnt?")]
    public GameObject lifePrefab;  

    [Header("Spawn-Einstellungen")]
    public float spawnEvery = 5.0f;
    public float spawnInset = 0.5f;
    public float spawnOffsetX = 1.0f;

    [Header("Bewegung")]
    public float speed = 3f;
    public Vector2 direction = Vector2.left;

    [Header("Aufräumen")]
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

        
        InvokeRepeating(nameof(Spawn), spawnEvery, spawnEvery);
    }
    void Update()
    {
    }

    void Spawn()
    {
        if (!lifePrefab) { Debug.LogWarning("LifeSpawner: lifePrefab fehlt."); return; }

        float y = Random.Range(camRect.yMin + spawnInset, camRect.yMax - spawnInset);
        Vector3 pos = new Vector3(rightSpawnX, y, 0f);

        var go = Instantiate(lifePrefab, pos, Quaternion.identity);
        var mover = go.GetComponent<LifeMover2D>();
        if (mover == null) mover = go.AddComponent<LifeMover2D>();

        mover.speed = speed * Random.Range(0.9f, 1.15f);
        mover.direction = direction;
        mover.SetBounds(camRect, despawnMargin);

        Debug.Log($"[LifeSpawner] Spawned {lifePrefab.name} at {pos}");
    }
}