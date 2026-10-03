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
using Tobii.Gaming;

public class PlayerMovement : MonoBehaviour
{
    [Header("Keyboard Movement")]
    public float moveSpeed = 5f;

    [Header("Eye Tracking (nur noch für evtl. spätere Features)")]
    [Tooltip("Kann aktuell ignoriert werden – Rotation per Blick ist deaktiviert.")]
    public bool useEyeTracking = false;
    [Range(0f, 0.2f)]
    public float screenPadding = 0.05f;
    [Tooltip("Optional: kleines Sprite, das deinen Blickpunkt zeigt (wird aktuell nicht benutzt).")]
    public Transform gazeMarker;

    [Header("Visuals")]
    [Tooltip("Standard-Rotation des Schiffs in Grad um Z (dein Sprite zeigt nach oben → -90).")]
    public float defaultRotationZ = -90f;

    private Rigidbody2D rb;
    private Camera cam;
    private float xMin, xMax, yMin, yMax;
    private float padX, padY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (TryGetComponent(out Collider2D col))
        {
            var e = col.bounds.extents;
            padX = e.x;
            padY = e.y;
        }
        else if (TryGetComponent(out SpriteRenderer sr))
        {
            var e = sr.bounds.extents;
            padX = e.x;
            padY = e.y;
        }
        else
        {
            padX = padY = 0.5f;
        }

        if (cam != null)
        {
            UpdateCameraBounds();
        }
        else
        {
            Debug.LogWarning("PlayerMovement: No camera with tag 'MainCamera' found. Please tag your camera.");
        }

        transform.rotation = Quaternion.Euler(0f, 0f, defaultRotationZ);
    }

    void Update()
    {
        float mx = Input.GetAxisRaw("Horizontal");
        float my = Input.GetAxisRaw("Vertical");

        if (rb != null)
            rb.velocity = new Vector2(mx, my).normalized * moveSpeed;
        else
            transform.position += new Vector3(mx, my, 0f) * moveSpeed * Time.deltaTime;

        if (cam != null)
            ClampToScreen();
    }

    private void ClampToScreen()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, xMin + padX, xMax - padX);
        pos.y = Mathf.Clamp(pos.y, yMin + padY, yMax - padY);
        transform.position = pos;
    }

    private void UpdateCameraBounds()
    {
        if (cam == null) return;

        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0f, 0f, zDist));
        Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1f, 1f, zDist));
        xMin = bl.x;
        yMin = bl.y;
        xMax = tr.x;
        yMax = tr.y;
    }
}
