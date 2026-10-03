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

public class EnemyFire : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.0f;

    [Header("Aiming")]
    [Tooltip("If ON, the bullet aims at the player once on spawn. If OFF, it just flies straight along its own 'up' direction.")]
    [SerializeField] private bool _aimAtPlayer = true;

    private Transform player;
    private Vector2 shootDirection;

    Rect camRect;
    float margin = 1f;

    float halfWidth = 0.5f;
    float halfHeight = 0.5f;

    void Awake()
    {
        var diff = DifficultyManager.GetSavedDifficulty();
        _aimAtPlayer = (diff == DifficultyManager.Difficulty.Hard);

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
            player = pObj.transform;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
            Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
            camRect = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
        }

        CacheExtents();

        if (_aimAtPlayer && player != null)
        {
            shootDirection = (player.position - transform.position).normalized;
        }
        else
        {
            shootDirection = transform.up;
        }
    }

    void CacheExtents()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            Bounds b = col.bounds;
            halfWidth = b.extents.x;
            halfHeight = b.extents.y;
            return;
        }

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Bounds b = sr.bounds;
            halfWidth = b.extents.x;
            halfHeight = b.extents.y;
        }
    }

    void Update()
    {
        transform.Translate(shootDirection * _speed * Time.deltaTime, Space.World);

        var p = transform.position;

        float left = p.x - halfWidth;
        float right = p.x + halfWidth;
        float bottom = p.y - halfHeight;
        float top = p.y + halfHeight;

        if (right < camRect.xMin - margin ||
            left > camRect.xMax + margin ||
            top < camRect.yMin - margin ||
            bottom > camRect.yMax + margin)
        {
            Destroy(gameObject);
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

            Destroy(this.gameObject);
        }
    }
}
