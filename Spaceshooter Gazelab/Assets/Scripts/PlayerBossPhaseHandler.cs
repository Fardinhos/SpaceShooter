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

public class PlayerBossPhaseHandler : MonoBehaviour
{
    [Header("Boss Intro Movement")]
    [Tooltip("Wie lange der Spieler zum zurückfliegen braucht (Sekunden).")]
    public float autoMoveDuration = 2.0f;
    private bool bossClampActive = false;
    private bool autoMoving = false;
    private Vector3 startPosition;
    private Camera cam;
    private float halfScreenX;

    private void Awake()
    {
        cam = Camera.main;
        startPosition = transform.position;
        UpdateHalfScreenX();
    }

    private void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            UpdateHalfScreenX();
        }
    }

    private void UpdateHalfScreenX()
    {
        if (cam == null) return;

        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 mid = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, zDist));
        halfScreenX = mid.x;
    }
    public void StartBossIntroMovement(float delaySeconds)
    {
        if (autoMoving || bossClampActive)
            return;
        StartCoroutine(BossIntroMovementRoutine(delaySeconds));
    }

    private IEnumerator BossIntroMovementRoutine(float delaySeconds)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        autoMoving = true;
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        Vector3 from = transform.position;
        Vector3 to = startPosition;
        float t = 0f;

        while (t < autoMoveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / autoMoveDuration);
            float smooth = k * k * (3f - 2f * k);

            Vector3 pos = Vector3.Lerp(from, to, smooth);
            transform.position = pos;

            yield return null;
        }
        transform.position = startPosition;

        autoMoving = false;

        if (movement != null)
            movement.enabled = true;

        bossClampActive = true;
    }

    private void LateUpdate()
    {
        if (!bossClampActive || autoMoving || cam == null)
            return;

        Vector3 pos = transform.position;
        if (pos.x > halfScreenX)
        {
            pos.x = halfScreenX;
            transform.position = pos;
        }
    }
}
