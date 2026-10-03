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

public class AsteroidHit : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.75f;

    [Header("Asteroid Settings")]
    [Tooltip("If TRUE, the asteroid will be destroyed when hit by bullets or the player.")]
    [SerializeField] private bool destroyOnHit = false;

    private AudioSource audioSource;

    private void Awake()
    {
        var diff = DifficultyManager.GetSavedDifficulty();
        destroyOnHit = (diff == DifficultyManager.Difficulty.Easy);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            PlayEffects();

            if (destroyOnHit)
                Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            PlayEffects();

            if (destroyOnHit)
                Destroy(gameObject, 0.1f);
        }
    }

    private void PlayEffects()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, soundVolume);
        }
    }
}
