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

public class AsteroidTutorialHitRelay : MonoBehaviour
{
    [Tooltip("Tag deiner Spieler-Kugeln")]
    public string bulletTag = "Bullet";

    [Tooltip("Tag deines Spielers")]
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other) return;

        if (other.CompareTag(bulletTag))
        {
            Destroy(other.gameObject);
            var dir = FindObjectOfType<TutorialDirector>();
            if (dir) dir.NotifyAsteroidHit();
        }
        else if (other.CompareTag(playerTag))
        {
            var dir = FindObjectOfType<TutorialDirector>();
            if (dir) dir.NotifyPlayerHit();
        }
    }
}