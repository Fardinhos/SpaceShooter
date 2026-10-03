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

public class PlatformMover2D : MonoBehaviour
{
    public float speed = 3f;
    public Vector2 direction = Vector2.left;

    Rect camRect;
    float margin = 1f;

    float halfWidth = 0.5f;
    float halfHeight = 0.5f;

    public void SetBounds(Rect r, float m)
    {
        camRect = r;
        margin = m;
        CacheExtents();
    }

    void Awake()
    {
        CacheExtents();
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
        Vector3 delta = (Vector3)direction.normalized * speed * Time.deltaTime;
        transform.position += delta;

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
}
