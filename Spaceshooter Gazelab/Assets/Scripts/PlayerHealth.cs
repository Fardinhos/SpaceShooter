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

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives (Base, will be overridden by Difficulty)")]
    [Tooltip("Basiswert – wird zur Laufzeit je nach Difficulty durch Easy/Hard-Werte ersetzt.")]
    public int lives = 3;

    [Header("Difficulty Settings")]
    [Tooltip("Start-Leben im EASY-Modus.")]
    public int livesEasy = 4;
    [Tooltip("Start-Leben im HARD-Modus.")]
    public int livesHard = 2;

    [Tooltip("Hit-Flash-Dauer im EASY-Modus (Invulnerability-Zeit).")]
    public float hitFlashTimeEasy = 0.15f;
    [Tooltip("Hit-Flash-Dauer im HARD-Modus (Invulnerability-Zeit).")]
    public float hitFlashTimeHard = 0.10f;

    [Header("Hit Flash & Invulnerability")]
    [Tooltip("Soll der Spieler bei einem Treffer kurz aufflashen?")]
    public bool flashOnHit = true;

    [Tooltip("Farbe während des Hits.")]
    public Color hitFlashColor = new Color(1f, 0.3f, 0.3f, 1f);

    [Tooltip("Dauer des Hit-Flashs in Sekunden. In dieser Zeit ist der Spieler unverwundbar.")]
    public float hitFlashTime = 0.12f;

    [Header("Hit Sound")]
    [Tooltip("Sound, der abgespielt wird, wenn der Spieler Schaden bekommt.")]
    public AudioClip hitSound;

    [Range(0f, 1f)]
    [Tooltip("Lautstärke des Hit-Sounds.")]
    public float hitSoundVolume = 1f;

    private bool _canBeHit = true;
    private SpriteRenderer[] _spriteRenderers;
    private Coroutine _flashCo;
    private AudioSource _audioSource;

    private void Awake()
    {
        var diff = DifficultyManager.GetSavedDifficulty();

        if (diff == DifficultyManager.Difficulty.Hard)
        {
            lives = Mathf.Max(1, livesHard);
            hitFlashTime = hitFlashTimeHard;
        }
        else
        {
            lives = Mathf.Max(1, livesEasy);
            hitFlashTime = hitFlashTimeEasy;
        }

        Debug.Log($"[PlayerHealth] Difficulty={diff}, lives={lives}, hitFlashTime={hitFlashTime}");
    }

    void Start()
    {
        UIManager.UpdateLives(lives);

        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
            Debug.LogWarning("[PlayerHealth] Kein SpriteRenderer gefunden – Hit-Flash wird unsichtbar sein.");
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Asteroid") ||
            other.CompareTag("Platform") ||
            other.CompareTag("EnemyBullet") ||
            other.CompareTag("Enemy") ||
            other.CompareTag("Boss"))
        {
            TakeHit();

            if (other.CompareTag("EnemyBullet"))
            {
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("Life"))
        {
            TakeLife();
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        TakeHit();
    }

    public void TakeHit()
    {
        if (!_canBeHit)
            return;

        PlayHitSound();

        lives--;
        Debug.Log($"[PlayerHealth] Player hit! Lives: {lives}");
        UIManager.UpdateLives(lives);

        if (lives <= 0)
        {
            Destroy(gameObject);
            GameOver();
            return;
        }

        if (flashOnHit)
        {
            if (_flashCo != null)
            {
                StopCoroutine(_flashCo);
            }
            _flashCo = StartCoroutine(HitFlashAndInvulRoutine());
        }
    }

    public void TakeLife()
    {
        lives++;
        Debug.Log($"[PlayerHealth] Extra life! Lives: {lives}");
        UIManager.UpdateLives(lives);
    }

    private void PlayHitSound()
    {
        if (hitSound == null || _audioSource == null)
            return;

        _audioSource.PlayOneShot(hitSound, hitSoundVolume);
    }

    IEnumerator HitFlashAndInvulRoutine()
    {
        _canBeHit = false;

        if (_spriteRenderers != null && _spriteRenderers.Length > 0)
        {
            Color[] originals = new Color[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                originals[i] = _spriteRenderers[i].color;
            }

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _spriteRenderers[i].color = hitFlashColor;
            }

            yield return new WaitForSeconds(hitFlashTime);

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _spriteRenderers[i].color = originals[i];
            }
        }
        else
        {
            yield return new WaitForSeconds(hitFlashTime);
        }

        _canBeHit = true;
        _flashCo = null;
    }

    private void GameOver()
    {
        Debug.Log("[PlayerHealth] Game Over – zurück ins Hauptmenü.");
        SceneManager.LoadScene("GameOver");
    }
}
