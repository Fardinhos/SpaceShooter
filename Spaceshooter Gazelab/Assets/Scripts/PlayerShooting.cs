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

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    [Header("Eye Aiming")]
    public bool useEyeAiming = true;
    public Camera cam;

    [Header("Aiming Crosshair")]
    public GameObject crosshairPrefab;
    public bool showCrosshair = true;
    [Tooltip("How smooth the crosshair follows your gaze (higher = smoother but slower)")]
    [Range(1f, 20f)]
    public float crosshairSmoothness = 10f;

    private GameObject crosshairInstance;
    private Vector3 lastCrosshairPos;

    [Header("Blink Shooting")]
    public bool shootByBlink = true;
    public float minBlinkDuration = 0.03f;
    public float maxBlinkDuration = 0.35f;
    public float blinkCooldown = 0.35f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showGazeDebug = true;

    private float cooldownTimer = 0f;
    private double lastValidTimestamp = 0;
    private float lastGazeTime = 0f;
    private float blinkStartTime = 0f;
    private bool wasGazeValid = false;

    private float gazeDebugTimer = 0f;
    private float gazeDebugInterval = 2f;
    private int validGazeCount = 0;
    private int invalidGazeCount = 0;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("PlayerShooting: No Camera found. Eye aiming off.");
            useEyeAiming = false;
        }

        if (firePoint == null)
            Debug.LogError("PlayerShooting: firePoint not assigned.");
        if (bulletPrefab == null)
            Debug.LogError("PlayerShooting: bulletPrefab not assigned.");

        if (crosshairPrefab != null && showCrosshair)
        {
            crosshairInstance = Instantiate(crosshairPrefab);
            crosshairInstance.name = "GazeCrosshair";
            lastCrosshairPos = Vector3.zero;
        }

        Debug.Log("=== TOBII EYE TRACKER DIAGNOSTICS ===");
        CheckTobiiStatus();
    }

    void OnDestroy()
    {
        if (crosshairInstance != null)
            Destroy(crosshairInstance);
    }

    void CheckTobiiStatus()
    {
        var userPresence = TobiiAPI.GetUserPresence();
        Debug.Log("Is User Present: " + userPresence.IsUserPresent().ToString());

        GazePoint gp = TobiiAPI.GetGazePoint();
        Debug.Log("Initial Gaze - IsRecent: " + gp.IsRecent().ToString() +
                  ", IsValid: " + gp.IsValid.ToString() +
                  ", Timestamp: " + gp.Timestamp.ToString());

        if (!gp.IsRecent())
        {
            Debug.LogWarning("NO RECENT TOBII GAZE DATA!");
            Debug.LogWarning("1. Open Tobii Experience/Eye Tracker software");
            Debug.LogWarning("2. Check if eye tracker LED is GREEN");
            Debug.LogWarning("3. Run calibration in Tobii software");
            Debug.LogWarning("4. Make sure you are in the tracking box");
            Debug.LogWarning("5. Restart Unity after calibrating");
            Debug.LogWarning("6. Check Windows Device Manager for Tobii device");
        }
        else
        {
            Debug.Log("SUCCESS: Tobii is providing gaze data!");
            lastValidTimestamp = gp.Timestamp;
            wasGazeValid = true;
        }

        Debug.Log("Screen resolution: " + Screen.width + "x" + Screen.height);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (showCrosshair && crosshairInstance != null && useEyeAiming && cam != null)
        {
            UpdateCrosshair();
        }

        if (showGazeDebug)
        {
            gazeDebugTimer += Time.deltaTime;
            if (gazeDebugTimer >= gazeDebugInterval)
            {
                gazeDebugTimer = 0f;
                LogGazeStatistics();
            }
        }

        if (shootByBlink)
            HandleBlinkShooting();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (cooldownTimer > 0f)
            {
                if (showDebugLogs)
                    Debug.Log("Space pressed but cooldown active");
                return;
            }

            cooldownTimer = blinkCooldown;

            if (useEyeAiming)
                ShootAtGaze();
            else
                ShootForward();
        }
    }

    void UpdateCrosshair()
    {
        GazePoint gazePoint = TobiiAPI.GetGazePoint();

        if (!gazePoint.IsRecent() || !gazePoint.IsValid)
        {
            if (crosshairInstance.activeSelf)
                crosshairInstance.SetActive(false);
            return;
        }

        if (!crosshairInstance.activeSelf)
            crosshairInstance.SetActive(true);

        Vector2 screenPos = gazePoint.Screen;
        Ray ray = cam.ScreenPointToRay(screenPos);
        float distance = Mathf.Abs(cam.transform.position.z - firePoint.position.z);
        Vector3 worldPos = ray.GetPoint(distance);
        worldPos.z = 0f;

        if (lastCrosshairPos == Vector3.zero)
            lastCrosshairPos = worldPos;

        lastCrosshairPos = Vector3.Lerp(lastCrosshairPos, worldPos, crosshairSmoothness * Time.deltaTime);
        crosshairInstance.transform.position = lastCrosshairPos;
    }

    void LogGazeStatistics()
    {
        GazePoint gp = TobiiAPI.GetGazePoint();
        var userPresence = TobiiAPI.GetUserPresence();

        Debug.Log("=== GAZE STATS (last " + gazeDebugInterval + "s) ===");
        Debug.Log("Valid frames: " + validGazeCount + " | Invalid frames: " + invalidGazeCount);
        Debug.Log("Current IsRecent: " + gp.IsRecent().ToString());
        Debug.Log("Current IsValid: " + gp.IsValid.ToString());
        Debug.Log("User Present: " + userPresence.IsUserPresent().ToString());
        Debug.Log("Screen Position: " + gp.Screen.ToString());
        Debug.Log("Timestamp: " + gp.Timestamp.ToString());

        validGazeCount = 0;
        invalidGazeCount = 0;
    }

    void HandleBlinkShooting()
    {
        if (firePoint == null || bulletPrefab == null)
            return;

        GazePoint gp = TobiiAPI.GetGazePoint();
        bool hasValidGaze = gp.IsValid && (gp.Timestamp > lastValidTimestamp);

        if (hasValidGaze)
            validGazeCount++;
        else
            invalidGazeCount++;

        if (hasValidGaze)
        {
            lastValidTimestamp = gp.Timestamp;
            lastGazeTime = Time.time;

            if (!wasGazeValid)
            {
                float blinkDuration = Time.time - blinkStartTime;

                if (showDebugLogs)
                {
                    Debug.Log("Gaze returned! Blink duration: " + blinkDuration.ToString("F3") +
                             "s (valid: " + minBlinkDuration.ToString("F3") +
                             "s - " + maxBlinkDuration.ToString("F3") + "s)");
                }

                if (blinkDuration >= minBlinkDuration && blinkDuration <= maxBlinkDuration)
                {
                    if (showDebugLogs)
                        Debug.Log(">>> VALID BLINK! Attempting to shoot...");
                    TryFireAfterBlink();
                }
                else
                {
                    if (showDebugLogs)
                    {
                        if (blinkDuration < minBlinkDuration)
                            Debug.Log("Blink too short - ignored");
                        else
                            Debug.Log("Blink too long - probably not a blink");
                    }
                }
            }

            wasGazeValid = true;
        }
        else
        {
            if (wasGazeValid)
            {
                blinkStartTime = Time.time;
                if (showDebugLogs)
                    Debug.Log(">>> Gaze lost - potential blink started");
            }

            wasGazeValid = false;
        }
    }

    void TryFireAfterBlink()
    {
        if (cooldownTimer > 0f)
        {
            if (showDebugLogs)
            {
                Debug.Log("Blink shoot BLOCKED by cooldown (" +
                         cooldownTimer.ToString("F2") + "s remaining)");
            }
            return;
        }

        cooldownTimer = blinkCooldown;

        if (showDebugLogs)
            Debug.Log("*** FIRING AFTER BLINK! ***");

        if (useEyeAiming)
            ShootAtGazeStrict();
        else
            ShootForward();
    }

    void ShootAtGazeStrict()
    {
        if (bulletPrefab == null || firePoint == null || cam == null)
            return;

        GazePoint gazePoint = TobiiAPI.GetGazePoint();
        if (!gazePoint.IsValid)
        {
            if (showDebugLogs)
                Debug.Log("No valid gaze for strict shooting - shooting forward instead");
            ShootForward();
            return;
        }

        Vector2 screenPos = gazePoint.Screen;
        Ray ray = cam.ScreenPointToRay(screenPos);

        float distance = Mathf.Abs(cam.transform.position.z - firePoint.position.z);
        Vector3 worldGazePos = ray.GetPoint(distance);

        Vector2 dir = ((Vector2)worldGazePos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

        Rigidbody2D rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.velocity = dir * bulletSpeed;

        Destroy(bullet, 5f);
    }

    void ShootAtGaze()
    {
        if (bulletPrefab == null || firePoint == null || cam == null)
            return;

        GazePoint gazePoint = TobiiAPI.GetGazePoint();
        if (!gazePoint.IsValid)
        {
            ShootForward();
            return;
        }

        Vector2 screenPos = gazePoint.Screen;
        Ray ray = cam.ScreenPointToRay(screenPos);

        float distance = Mathf.Abs(cam.transform.position.z - firePoint.position.z);
        Vector3 worldPos = ray.GetPoint(distance);

        Vector2 dir = ((Vector2)worldPos - (Vector2)firePoint.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

        Rigidbody2D rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.velocity = dir * bulletSpeed;

        Destroy(bullet, 5f);
    }

    void ShootForward()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Vector2 dir = firePoint.up;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

        Rigidbody2D rb2d = bullet.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.velocity = dir * bulletSpeed;

        Destroy(bullet, 5f);
    }
}
