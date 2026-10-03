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

public class Enemy : MonoBehaviour
{
    [Header("Movement (Base)")]
    [SerializeField]
    private float _speed = 4.0f;

    [Header("Shooting (Base)")]
    [SerializeField]
    private GameObject _laserPrefab; 

    [SerializeField]
    private float _minFireRate = 1.0f; 
    [SerializeField]
    private float _maxFireRate = 3.0f; 

    [Header("Difficulty Settings")]
    [Tooltip("Bewegungsgeschwindigkeit im EASY-Modus.")]
    [SerializeField] private float _speedEasy = 3.5f;
    [Tooltip("Bewegungsgeschwindigkeit im HARD-Modus.")]
    [SerializeField] private float _speedHard = 5.0f;

    [Tooltip("Min. Feuerrate (Sekunden) im EASY-Modus.")]
    [SerializeField] private float _minFireRateEasy = 1.2f;
    [Tooltip("Max. Feuerrate (Sekunden) im EASY-Modus.")]
    [SerializeField] private float _maxFireRateEasy = 3.0f;

    [Tooltip("Min. Feuerrate (Sekunden) im HARD-Modus.")]
    [SerializeField] private float _minFireRateHard = 0.6f;
    [Tooltip("Max. Feuerrate (Sekunden) im HARD-Modus.")]
    [SerializeField] private float _maxFireRateHard = 2.0f;

    [Header("Scoring")]
    [SerializeField]
    private int _points = 100; 

    [Header("Explosion Effect")]
    [SerializeField]
    private GameObject _explosionPrefab; 

    [Header("Sound")]
    [SerializeField] private AudioClip _hitSound;
    [Range(0f, 1f)]
    [SerializeField] private float _hitVolume = 0.8f;

    private bool _isAlive = true;


    private void Awake()
    {
        ApplyDifficultySettings();
    }

    void Start()
    {
        StartCoroutine(FireLaserRoutine());
    }

    private void ApplyDifficultySettings()
    {
        var diff = DifficultyManager.GetSavedDifficulty();

        if (diff == DifficultyManager.Difficulty.Hard)
        {
            _speed       = _speedHard;
            _minFireRate = _minFireRateHard;
            _maxFireRate = _maxFireRateHard;
        }
        else
        {
            _speed       = _speedEasy;
            _minFireRate = _minFireRateEasy;
            _maxFireRate = _maxFireRateEasy;
        }

        if (_minFireRate > _maxFireRate)
        {
            float tmp = _minFireRate;
            _minFireRate = _maxFireRate;
            _maxFireRate = tmp;
        }

        Debug.Log($"[Enemy] Difficulty={diff}, speed={_speed}, fireRate=({_minFireRate}..{_maxFireRate})");
    }

    void Update()
    {
        transform.Translate(Vector3.up * _speed * Time.deltaTime);

        if (transform.position.x < -11.0f)
        {
            float randomY = Random.Range(-4.5f, 4.5f); 
            transform.position = new Vector3(11.0f, randomY, 0);
        }
    }

    IEnumerator FireLaserRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        while (_isAlive == true)
        {
            Instantiate(_laserPrefab, transform.position, _laserPrefab.transform.rotation);

            float randomDelay = Random.Range(_minFireRate, _maxFireRate);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeHit();
            }

            _isAlive = false;

            if (_explosionPrefab != null)
            {
                Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            }

            Destroy(this.gameObject);
        }
        else if (other.CompareTag("Bullet")) 
        {
            Destroy(other.gameObject); 
            _isAlive = false;

            UIManager.AddScore(_points); 

            if (_hitSound != null)
            {
                AudioSource.PlayClipAtPoint(_hitSound, transform.position, _hitVolume);
            }

            if (_explosionPrefab != null)
            {
                Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            }

            Destroy(this.gameObject);
        }
    }
}
