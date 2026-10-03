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

public class PlatformHit : MonoBehaviour
{
    [Header("Treffer & Punkte")]
    public int hitsToDestroy = 1;
    public int points = 50;

    [Header("Effekte")]
    public GameObject explosionPrefab;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float hitVolume = 0.8f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            hitsToDestroy--;

            PlayHitEffects();

            if (hitsToDestroy <= 0)
            {
                int reward = Mathf.Abs(points);
                UIManager.AddScore(reward);

                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            PlayHitEffects();
        }
    }

    private void PlayHitEffects()
    {
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitVolume);
        }
    }
}
