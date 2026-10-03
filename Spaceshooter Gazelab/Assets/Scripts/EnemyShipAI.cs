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

public class EnemyShipAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public Vector2 moveDir = Vector2.left;

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireInterval = 1.2f;
    public float bulletSpeed = 6f;

    [Header("Hit / Death")]
    public string playerBulletTag = "Bullet";
    public GameObject explosionPrefab;

    public enum SpriteForward { Up, Right, Down, Left }

    [Header("Auto FirePoint & Ausrichtung")]
    public bool autoPlaceFirePoint = true;
    public SpriteForward spriteLooksTo = SpriteForward.Up;
    public float noseOffset = 0.05f;
    public bool rotateSpriteToLeft = true;

    public enum BulletForward { Up, Right, Down, Left }
    [Header("Bullet Orientation")]
    public BulletForward bulletMovesAlong = BulletForward.Up;

    Rigidbody2D rb;

    public void InitMoveLeft(float speed) { moveDir = Vector2.left; moveSpeed = speed; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.gravityScale = 0f;

        if (rotateSpriteToLeft) FaceLeftVisually();
        if (autoPlaceFirePoint) EnsureAndPlaceFirePoint();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!UnityEditor.EditorApplication.isPlaying && autoPlaceFirePoint)
            EnsureAndPlaceFirePoint();
    }
#endif

    void OnEnable() => StartCoroutine(FireLoop());

    void Update()
    {
        Vector3 d = (Vector3)moveDir.normalized * moveSpeed * Time.deltaTime;
        if (rb && rb.bodyType == RigidbodyType2D.Dynamic) rb.MovePosition(rb.position + (Vector2)d);
        else transform.position += d;
    }

    IEnumerator FireLoop()
    {
        yield return new WaitForSeconds(0.4f);
        while (true)
        {
            ShootLeft();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void ShootLeft()
    {
        if (!bulletPrefab || !firePoint) return;

        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Vector2 axis =
            (bulletMovesAlong == BulletForward.Up) ? Vector2.up :
            (bulletMovesAlong == BulletForward.Right) ? Vector2.right :
            (bulletMovesAlong == BulletForward.Down) ? Vector2.down :
                                                        Vector2.right;

        float angle = Vector2.SignedAngle(axis, Vector2.left);
        b.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var brb = b.GetComponent<Rigidbody2D>();
        if (brb)
        {
            brb.gravityScale = 0f;
            brb.velocity = Vector2.left * bulletSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || !other.CompareTag(playerBulletTag)) return;

        Destroy(other.gameObject);

        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        var dir = FindObjectOfType<TutorialDirector>();
        if (dir) dir.NotifyEnemyDestroyed(transform.position);

        Destroy(gameObject);
    }


    void EnsureAndPlaceFirePoint()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (!sr || sr.sprite == null) return;

        if (!firePoint)
        {
            var go = new GameObject("FirePoint");
            firePoint = go.transform;
            firePoint.SetParent(sr.transform, false);
        }
        else if (firePoint.parent != sr.transform)
        {
            firePoint.SetParent(sr.transform, true);
        }

        Vector2 ext = sr.sprite.bounds.extents;
        Vector3 dir =
            (spriteLooksTo == SpriteForward.Up) ? Vector3.up :
            (spriteLooksTo == SpriteForward.Right) ? Vector3.right :
            (spriteLooksTo == SpriteForward.Down) ? Vector3.down :
                                                     Vector3.left;

        float dist = (dir == Vector3.up || dir == Vector3.down) ? ext.y : ext.x;
        firePoint.localPosition = dir * (dist + noseOffset);
        firePoint.localRotation = Quaternion.identity;
        firePoint.localScale = Vector3.one;
    }

    void FaceLeftVisually()
    {
        float z =
            (spriteLooksTo == SpriteForward.Up) ? 90f :
            (spriteLooksTo == SpriteForward.Right) ? 180f :
            (spriteLooksTo == SpriteForward.Down) ? -90f :
                                                     0f;
        transform.rotation = Quaternion.Euler(0f, 0f, z);
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint) Gizmos.DrawWireSphere(firePoint.position, 0.06f);
    }
}