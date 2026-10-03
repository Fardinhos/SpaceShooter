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
using UnityEngine.SceneManagement;


public class Boss : MonoBehaviour
{
    public enum FireMode { AimedSingle, AimedSpread, RadialRing }
    public enum ArcSide { Top, Bottom, Left, Right }
    public enum BulletSelectMode { UseA, UseB, Alternate, RandomWeighted }

    [Header("Stats")]
    public int health = 50;

    [Header("Einflug")]
    public float entrySpeed = 3f;
    public float stopMargin = 0.25f;

    [Header("Vertikale Bewegung")]
    public float patrolRangeY = 3f;
    public float patrolSpeed = 2f;

    [Header("Schießen (Basis)")]
    [Tooltip("Enemy-Bullet Prefab A (z.B. Laser, Rigidbody2D (Gravity=0) + Collider2D).")]
    public GameObject enemyBulletPrefabA;

    [Tooltip("Enemy-Bullet Prefab B (z.B. Rakete).")]
    public GameObject enemyBulletPrefabB;

    [Tooltip("Basis-Projektilgeschwindigkeit.")]
    public float bulletSpeed = 7f;

    [Tooltip("Basis-Feuerrate (Sekunden).")]
    public float fireRate = 1.0f;

    [Header("Bullet-Auswahl")]
    public BulletSelectMode bulletSelectMode = BulletSelectMode.UseA;

    [Tooltip("Gewicht für Prefab A (nur für RandomWeighted).")]
    [Range(0f, 10f)] public float weightA = 1f;

    [Tooltip("Gewicht für Prefab B (nur für RandomWeighted).")]
    [Range(0f, 10f)] public float weightB = 1f;

    [Header("Audio (Schuss-Sounds)")]
    [Tooltip("AudioSource nur für Boss-SFX (nicht für Musik). Wenn leer, wird einer automatisch angelegt.")]
    public AudioSource sfxSource;

    [Header("Laser-Sounds (Prefab A)")]
    public AudioClip laserSingleClip;
    public AudioClip laserMultiClip;

    [Header("Raketen-Sounds (Prefab B)")]
    public AudioClip rocketSingleClip;
    public AudioClip rocketMultiClip;

    [Header("Random Projectile Speed")]
    [Tooltip("Wenn aktiv, variiert die Projektilgeschwindigkeit pro Schuss zufällig.")]
    public bool randomizeProjectileSpeed = true;
    [Tooltip("Multiplikator-Bereich auf bulletSpeed.")]
    public Vector2 speedJitterMultiplierRange = new Vector2(0.85f, 1.25f);

    [Header("Random Fire Cadence (optional)")]
    [Tooltip("Wenn aktiv, variiert die Feuerrate pro Schuss.")]
    public bool randomizeFireRate = false;
    public Vector2 fireRateRange = new Vector2(0.8f, 1.4f);

    [Header("Fire Patterns")]
    [Tooltip("Aktiver Schießmodus (kann automatisch wechseln).")]
    public FireMode currentFireMode = FireMode.AimedSingle;

    [Tooltip("Automatisch zwischen Mustern wechseln.")]
    public bool autoSwitchPatterns = true;

    [Tooltip("Dauer je Muster (Sekunden).")]
    public Vector2 patternDurationRange = new Vector2(6f, 10f);

    [Tooltip("AimedSpread: Winkelabstand (Grad) links/rechts vom Ziel.")]
    public float spreadAngle = 12f;

    [Tooltip("RadialRing: Anzahl Projektile pro Schuss.")]
    public int ringProjectiles = 12;

    [Tooltip("Startwinkel (Grad) für den Ring.")]
    public float ringStartAngle = 0f;

    [Tooltip("Zusätzliche Rotation (Grad) pro Ring-Schuss.")]
    public float ringSpinPerShot = 15f;

    [Header("Pattern Wahrscheinlichkeiten")]
    [Tooltip("Wahrscheinlichkeit für RadialRing (Rundumschlag), wenn Schild unten und vorher kein Ring.")]
    [Range(0f, 1f)] public float radialRingChance = 0.6f;

    [Tooltip("Wahrscheinlichkeit für AimedSpread (Dreifachschuss). AimedSingle bekommt den Rest.")]
    [Range(0f, 1f)] public float aimedSpreadChance = 0.2f;

    [Header("Schild (bricht + lädt nach)")]
    public bool useShield = true;
    public GameObject shieldVisual;
    public int hitsToBreakShield = 10;
    public float shieldRechargeDelay = 15f;

    [Header("Schild-Rendering (Halbkreis + optional kleiner Kreis)")]
    public bool autoGenerateShieldShapes = true;
    public ArcSide arcSide = ArcSide.Top;
    [Range(0.2f, 1.5f)] public float arcRadiusScale = 0.7f;
    public float shieldRadiusPadding = 0.0f;
    public float arcWidth = 0.10f;
    public int arcSegments = 64;

    [Tooltip("Kleinen Innenkreis zusätzlich zeichnen?")]
    public bool drawInnerCircle = false;
    [Range(0.05f, 0.95f)] public float innerCircleScale = 0.6f;
    public float innerWidth = 0.08f;
    public int innerSegments = 64;

    [Header("Schild-Effekte")]
    public bool pulseWhileActive = true;
    public float pulseSpeed = 6f;
    [Range(0f, 0.3f)] public float pulseScale = 0.12f;

    [Header("Schild Schaden-Visual")]
    [Tooltip("Farbe bei vollem Schild.")]
    public Color shieldFullColor = new Color(0f, 1f, 1f, 0.9f);
    [Tooltip("Farbe kurz vor dem Zusammenbruch (rötlicher, transparenter).")]
    public Color shieldLowColor = new Color(1f, 0.3f, 0.3f, 0.3f);
    [Tooltip("Dauer des hellen Aufblinkens bei einem Treffer.")]
    public float shieldHitFlashDuration = 0.12f;

    [Header("Feedback & Debug")]
    public bool flashOnToggle = true;
    [Range(0f, 1f)] public float flashAlpha = 0.35f;
    public float flashTime = 0.12f;
    public bool showLogs = true;

    private Camera cam;
    private Rigidbody2D rb;
    private bool hasEntered = false;
    private float stopX;
    private float halfWidth = 0.5f, halfHeight = 0.5f;
    private Coroutine shootLoopCo, patternLoopCo;

    private float patrolCenterY;
    private bool movingUp = true;

    private bool shieldActive = false;
    private bool isRechargingShield = false;
    private int shieldHitCounter = 0;

    private SpriteRenderer[] spriteRenderers;
    private Vector3 shieldVisualBaseScale = Vector3.one;

    private Transform shieldGroup;
    private LineRenderer arcLR;
    private LineRenderer innerLR;
    private float arcBaseWidth, innerBaseWidth;

    private bool toggleAB = false;

    private Coroutine shieldFlashCo;

    private enum ShotContext { Single, Spread, Ring }

    void OnValidate()
    {
        if (speedJitterMultiplierRange.x > speedJitterMultiplierRange.y)
            speedJitterMultiplierRange = new Vector2(speedJitterMultiplierRange.y, speedJitterMultiplierRange.x);
        if (speedJitterMultiplierRange.x < 0f) speedJitterMultiplierRange.x = 0f;

        if (fireRateRange.x > fireRateRange.y)
            fireRateRange = new Vector2(fireRateRange.y, fireRateRange.x);
        if (fireRateRange.x < 0.05f) fireRateRange.x = 0.05f;

        if (patternDurationRange.x > patternDurationRange.y)
            patternDurationRange = new Vector2(patternDurationRange.y, patternDurationRange.x);
        if (patternDurationRange.x < 0.2f) patternDurationRange.x = 0.2f;

        ringProjectiles = Mathf.Max(3, ringProjectiles);
        arcSegments = Mathf.Max(8, arcSegments);
        innerSegments = Mathf.Max(8, innerSegments);
        fireRate = Mathf.Max(0.05f, fireRate);
        bulletSpeed = Mathf.Max(0.01f, bulletSpeed);

        radialRingChance = Mathf.Clamp01(radialRingChance);
        aimedSpreadChance = Mathf.Clamp01(aimedSpreadChance);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if (TryGetComponent(out Collider2D col)) { halfWidth = col.bounds.extents.x; halfHeight = col.bounds.extents.y; }
        else if (TryGetComponent(out SpriteRenderer sr)) { halfWidth = sr.bounds.extents.x; halfHeight = sr.bounds.extents.y; }

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (shieldVisual != null)
        {
            shieldVisualBaseScale = shieldVisual.transform.localScale;
            shieldVisual.SetActive(false);
        }

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        ApplyDifficultySettings();
    }

    void ApplyDifficultySettings()
    {
        var diff = DifficultyManager.GetSavedDifficulty();

        if (diff == DifficultyManager.Difficulty.Hard)
        {
            health = Mathf.RoundToInt(health * 1.3f);              
            fireRate *= 0.8f;                                      
            bulletSpeed *= 1.1f;                                   

            hitsToBreakShield = Mathf.Max(1, Mathf.RoundToInt(hitsToBreakShield * 0.8f));
            radialRingChance = Mathf.Clamp01(radialRingChance + 0.15f);                   
            aimedSpreadChance = Mathf.Clamp01(aimedSpreadChance + 0.05f);                 

            if (showLogs) Debug.Log("[Boss] Difficulty HARD applied.");
        }
        else
        {
            health = Mathf.RoundToInt(health * 0.7f);              
            fireRate *= 1.2f;                                      
            bulletSpeed *= 0.9f;                                   

            hitsToBreakShield = Mathf.Max(1, Mathf.RoundToInt(hitsToBreakShield * 1.3f)); 
            radialRingChance = Mathf.Clamp01(radialRingChance - 0.2f);                 

            if (showLogs) Debug.Log("[Boss] Difficulty EASY applied.");
        }
    }

    void Start()
    {
        float zDist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float camRightX = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, zDist)).x;
        stopX = camRightX - halfWidth - stopMargin;

        if (autoGenerateShieldShapes) BuildAutoShieldShapes();
        if (useShield) SetShield(true);

        if (showLogs) Debug.Log($"[Boss] Start @ {transform.position}, stopX = {stopX:F2}");

        shootLoopCo = StartCoroutine(ShootLoop());
        if (autoSwitchPatterns && patternLoopCo == null)
            patternLoopCo = StartCoroutine(PatternSwitcher());
    }

    void Update()
    {
        if (!hasEntered)
        {
            transform.Translate(Vector3.left * entrySpeed * Time.deltaTime, Space.World);
            if (rb) rb.velocity = new Vector2(-entrySpeed, 0f);

            if (transform.position.x <= stopX)
            {
                hasEntered = true;
                transform.position = new Vector3(stopX, transform.position.y, transform.position.z);
                if (rb) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; }

                patrolCenterY = transform.position.y;
                if (showLogs) Debug.Log("[Boss] Voll im Bild → Patrouille & Schießen.");

                if (MusicManager.Instance != null)
                {
                    MusicManager.Instance.PlayBossMusic();
                }
            }
            return;
        }

        float topY = patrolCenterY + patrolRangeY;
        float bottomY = patrolCenterY - patrolRangeY;
        if (movingUp)
        {
            transform.Translate(Vector3.up * patrolSpeed * Time.deltaTime, Space.World);
            if (transform.position.y >= topY) movingUp = false;
        }
        else
        {
            transform.Translate(Vector3.down * patrolSpeed * Time.deltaTime, Space.World);
            if (transform.position.y <= bottomY) movingUp = true;
        }

        if (pulseWhileActive && shieldActive)
        {
            float p = 1f + pulseScale * Mathf.Sin(Time.time * pulseSpeed);
            if (arcLR) arcLR.widthMultiplier = arcBaseWidth * p;
            if (innerLR) innerLR.widthMultiplier = innerBaseWidth * p;
        }
        else
        {
            if (arcLR) arcLR.widthMultiplier = arcBaseWidth;
            if (innerLR) innerLR.widthMultiplier = innerBaseWidth;
        }
    }

    IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (hasEntered)
                FireAccordingToPattern();

            float wait = fireRate;
            if (randomizeFireRate)
            {
                float min = Mathf.Min(fireRateRange.x, fireRateRange.y);
                float max = Mathf.Max(fireRateRange.x, fireRateRange.y);
                wait = Random.Range(min, max);
            }
            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator PatternSwitcher()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        while (true)
        {
            float durMin = Mathf.Max(0.2f, Mathf.Min(patternDurationRange.x, patternDurationRange.y));
            float durMax = Mathf.Max(durMin, Mathf.Max(patternDurationRange.x, patternDurationRange.y));
            float dur = Random.Range(durMin, durMax);

            FireMode next = currentFireMode;

            float pRing = Mathf.Clamp01(radialRingChance);
            float pSpread = Mathf.Clamp01(aimedSpreadChance);
            float pSingle = Mathf.Max(0f, 1f - (pRing + pSpread));

            if (useShield && shieldActive)
            {
                float total = pSingle + pSpread;
                if (total <= 0f)
                {
                    next = FireMode.AimedSingle;
                }
                else
                {
                    float r = Random.value * total;
                    if (r < pSpread)
                        next = FireMode.AimedSpread;
                    else
                        next = FireMode.AimedSingle;
                }
            }
            else
            {
                if (currentFireMode == FireMode.RadialRing)
                {
                    float total = pSingle + pSpread;
                    if (total <= 0f)
                    {
                        next = FireMode.AimedSingle;
                    }
                    else
                    {
                        float r = Random.value * total;
                        if (r < pSpread)
                            next = FireMode.AimedSpread;
                        else
                            next = FireMode.AimedSingle;
                    }
                }
                else
                {
                    float r = Random.value;
                    if (r < pRing)
                        next = FireMode.RadialRing;
                    else if (r < pRing + pSpread)
                        next = FireMode.AimedSpread;
                    else
                        next = FireMode.AimedSingle;
                }
            }

            currentFireMode = next;

            yield return new WaitForSeconds(dur);
        }
    }

    void FireAccordingToPattern()
    {

        if (useShield && shieldActive && currentFireMode == FireMode.RadialRing)
        {
            currentFireMode = (Random.value < 0.5f) ? FireMode.AimedSingle : FireMode.AimedSpread;
        }

        switch (currentFireMode)
        {
            case FireMode.AimedSingle:
                Fire_AimedSingle();
                break;
            case FireMode.AimedSpread:
                Fire_AimedSpread();
                break;
            case FireMode.RadialRing:
                Fire_RadialRing();
                break;
        }
    }

    void Fire_AimedSingle()
    {
        Vector3 firePos = transform.position;
        Transform player = FindPlayer();
        Vector2 dir = player ? ((Vector2)player.position - (Vector2)firePos).normalized : Vector2.left;

        GameObject prefab = PickBulletPrefab();

        PlayShotSound(prefab, ShotContext.Single);

        SpawnDirectionalBullet(firePos, dir, prefab);
    }

    void Fire_AimedSpread()
    {
        Vector3 firePos = transform.position;
        Transform player = FindPlayer();
        Vector2 baseDir = player ? ((Vector2)player.position - (Vector2)firePos).normalized : Vector2.left;

        GameObject prefab = PickBulletPrefab();

        PlayShotSound(prefab, ShotContext.Spread);

        SpawnDirectionalBullet(firePos, baseDir, prefab);
        Quaternion qL = Quaternion.Euler(0f, 0f, spreadAngle);
        Quaternion qR = Quaternion.Euler(0f, 0f, -spreadAngle);
        SpawnDirectionalBullet(firePos, qL * baseDir, prefab);
        SpawnDirectionalBullet(firePos, qR * baseDir, prefab);
    }

    void Fire_RadialRing()
    {
        Vector3 firePos = transform.position;
        int n = Mathf.Max(3, ringProjectiles);

        GameObject prefab = PickBulletPrefab();

        PlayShotSound(prefab, ShotContext.Ring);

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)n;
            float angDeg = ringStartAngle + t * 360f;
            float rad = angDeg * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            SpawnDirectionalBullet(firePos, dir, prefab);
        }
        ringStartAngle += ringSpinPerShot;
    }

    GameObject PickBulletPrefab()
    {
        GameObject a = enemyBulletPrefabA;
        GameObject b = enemyBulletPrefabB;

        switch (bulletSelectMode)
        {
            case BulletSelectMode.UseA:
                return a != null ? a : (b != null ? b : null);

            case BulletSelectMode.UseB:
                return b != null ? b : (a != null ? a : null);

            case BulletSelectMode.Alternate:
                toggleAB = !toggleAB;
                if (toggleAB)
                    return a != null ? a : b;
                else
                    return b != null ? b : a;

            case BulletSelectMode.RandomWeighted:
                float wA = (a != null) ? Mathf.Max(0f, weightA) : 0f;
                float wB = (b != null) ? Mathf.Max(0f, weightB) : 0f;
                float sum = wA + wB;
                if (sum <= 0f) return a != null ? a : b;

                float r = Random.value * sum;
                return (r < wA) ? a : b;
        }
        return a != null ? a : b;
    }

    void PlayShotSound(GameObject projectilePrefab, ShotContext context)
    {
        if (sfxSource == null || projectilePrefab == null)
            return;

        AudioClip clip = null;

        if (projectilePrefab == enemyBulletPrefabA)
        {
            clip = (context == ShotContext.Single) ? laserSingleClip : laserMultiClip;
        }
        else if (projectilePrefab == enemyBulletPrefabB)
        {
            clip = (context == ShotContext.Single) ? rocketSingleClip : rocketMultiClip;
        }

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void SpawnDirectionalBullet(Vector3 pos, Vector2 dir, GameObject projectilePrefab)
    {
        if (projectilePrefab == null) return;

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.AngleAxis(ang, Vector3.forward);

        GameObject b = Instantiate(projectilePrefab, pos, rot);

        float spd = bulletSpeed;
        if (randomizeProjectileSpeed)
        {
            float jMin = Mathf.Min(speedJitterMultiplierRange.x, speedJitterMultiplierRange.y);
            float jMax = Mathf.Max(speedJitterMultiplierRange.x, speedJitterMultiplierRange.y);
            spd *= Random.Range(jMin, jMax);
        }

        var rb2d = b.GetComponent<Rigidbody2D>();
        if (rb2d) rb2d.velocity = dir.normalized * spd;

        if (b.tag == "Untagged") b.tag = "EnemyBullet";

        Destroy(b, 6f);
    }

    Transform FindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go ? go.transform : null;
    }

    public void TakeHit(int amount = 1)
    {
        if (health <= 0) return;

        if (shieldActive)
        {
            shieldHitCounter++;
            if (showLogs) Debug.Log($"[Boss] Schild absorbiert Treffer ({shieldHitCounter}/{hitsToBreakShield})");

            UpdateShieldVisual();

            if (shieldHitFlashDuration > 0f)
            {
                if (shieldFlashCo != null) StopCoroutine(shieldFlashCo);
                shieldFlashCo = StartCoroutine(ShieldHitFlashRoutine());
            }

            if (shieldHitCounter >= hitsToBreakShield)
                BreakShield();
            else if (flashOnToggle)
                StartCoroutine(Flash(Color.cyan, flashTime));

            return;
        }

        health -= Mathf.Max(1, amount);
        if (showLogs) Debug.Log($"[Boss] Treffer! Leben: {health}");

        if (health > 0 && flashOnToggle)
        {
            StartCoroutine(Flash(Color.white, flashTime));
        }

        if (health <= 0) Die();
    }

    void BreakShield()
    {
        if (!shieldActive) return;
        SetShield(false);
        if (showLogs) Debug.Log("[Boss] Schild GEBROCHEN – Boss verwundbar!");
        if (useShield && gameObject.activeInHierarchy)
            StartCoroutine(RechargeShieldAfterDelayRealtime(shieldRechargeDelay));
    }

    IEnumerator RechargeShieldAfterDelayRealtime(float delay)
    {
        if (isRechargingShield) yield break;
        isRechargingShield = true;

        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, delay));
        if (!this || !gameObject.activeInHierarchy || health <= 0) { isRechargingShield = false; yield break; }

        SetShield(true);
        if (showLogs) Debug.Log("[Boss] Schild wieder AKTIV.");
        isRechargingShield = false;
    }

    void SetShield(bool active)
    {
        shieldActive = active;
        if (active) shieldHitCounter = 0;

        if (shieldVisual)
        {
            shieldVisual.SetActive(active);
            shieldVisual.transform.localScale = shieldVisualBaseScale;
        }

        if (shieldGroup) shieldGroup.gameObject.SetActive(active);

        if (flashOnToggle) StartCoroutine(Flash(active ? Color.cyan : Color.white, flashTime));

        if (active)
        {
            UpdateShieldVisual();
        }
    }

    void UpdateShieldVisual()
    {
        if (!shieldActive) return;

        float t = 0f;
        if (hitsToBreakShield > 0)
            t = Mathf.Clamp01((float)shieldHitCounter / hitsToBreakShield);

        Color c = Color.Lerp(shieldFullColor, shieldLowColor, t);

        if (arcLR != null)
        {
            arcLR.startColor = c;
            arcLR.endColor = c;
        }

        if (innerLR != null)
        {
            innerLR.startColor = c;
            innerLR.endColor = c;
        }

        if (shieldVisual != null)
        {
            var sr = shieldVisual.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = c;
        }
    }

    IEnumerator ShieldHitFlashRoutine()
    {
        if (arcLR == null && innerLR == null)
            yield break;

        Color arcStart = arcLR != null ? arcLR.startColor : Color.white;
        Color innerStart = innerLR != null ? innerLR.startColor : Color.white;

        float t = 0f;
        while (t < shieldHitFlashDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.PI * (t / shieldHitFlashDuration));

            Color flashColorArc = Color.Lerp(arcStart, Color.white, k);
            Color flashColorInner = Color.Lerp(innerStart, Color.white, k);

            if (arcLR != null)
            {
                arcLR.startColor = flashColorArc;
                arcLR.endColor = flashColorArc;
            }
            if (innerLR != null)
            {
                innerLR.startColor = flashColorInner;
                innerLR.endColor = flashColorInner;
            }

            yield return null;
        }

        UpdateShieldVisual();
        shieldFlashCo = null;
    }

    IEnumerator Flash(Color tint, float t)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) yield break;

        var originals = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++) originals[i] = spriteRenderers[i].color;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var c = tint;
            c.a = Mathf.Clamp01(originals[i].a * (1f - flashAlpha));
            spriteRenderers[i].color = c;
        }
        yield return new WaitForSeconds(t);
        for (int i = 0; i < spriteRenderers.Length; i++) spriteRenderers[i].color = originals[i];
    }

    void Die()
    {
        if (shootLoopCo != null) StopCoroutine(shootLoopCo);
        if (patternLoopCo != null) StopCoroutine(patternLoopCo);
        if (showLogs) Debug.Log("[Boss] Besiegt!");
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayGameplayMusic();
        }

        Destroy(gameObject, 0.5f);
        SceneManager.LoadScene("Won");

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other) return;
        if (other.CompareTag("Bullet"))
        {
            if (shieldActive)
            {
                Destroy(other.gameObject);
                TakeHit(0);                
            }
            else
            {
                TakeHit();
                Destroy(other.gameObject);
            }
        }

        if (other.CompareTag("Player"))
        {
            var hp = other.GetComponent<PlayerHealth>();
            if (hp) hp.TakeHit();
        }
    }

    void BuildAutoShieldShapes()
    {
        var groupGO = new GameObject("AutoShieldGroup");
        shieldGroup = groupGO.transform;
        shieldGroup.SetParent(transform);
        shieldGroup.localPosition = Vector3.zero;
        shieldGroup.localRotation = Quaternion.identity;
        shieldGroup.localScale = Vector3.one;

        var mat = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };
        Color c = shieldFullColor;

        float baseRadius = Mathf.Max(halfWidth, halfHeight) + shieldRadiusPadding;
        float radius = Mathf.Max(0.05f, baseRadius * arcRadiusScale);

        arcLR = groupGO.AddComponent<LineRenderer>();
        arcLR.loop = false;
        arcLR.useWorldSpace = false;
        arcLR.material = mat;
        arcLR.startColor = c; arcLR.endColor = c;
        arcLR.widthMultiplier = arcWidth; arcBaseWidth = arcWidth;
        arcLR.textureMode = LineTextureMode.Stretch;
        arcLR.alignment = LineAlignment.View;
        arcLR.sortingLayerID = SortingLayer.NameToID("Default");
        arcLR.sortingOrder = 50;

        Vector2 ang = GetArcAngles(arcSide);
        int count = Mathf.Max(8, arcSegments) + 1;
        arcLR.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            float a = Mathf.Lerp(ang.x, ang.y, t);
            Vector3 p = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            arcLR.SetPosition(i, p);
        }

        if (drawInnerCircle)
        {
            innerLR = new GameObject("InnerCircle").AddComponent<LineRenderer>();
            innerLR.transform.SetParent(shieldGroup);
            innerLR.transform.localPosition = Vector3.zero;
            innerLR.transform.localRotation = Quaternion.identity;
            innerLR.transform.localScale = Vector3.one;

            innerLR.loop = true;
            innerLR.useWorldSpace = false;
            innerLR.material = mat;
            innerLR.startColor = c; innerLR.endColor = c;
            innerLR.widthMultiplier = innerWidth; innerBaseWidth = innerWidth;
            innerLR.textureMode = LineTextureMode.Stretch;
            innerLR.alignment = LineAlignment.View;
            innerLR.sortingLayerID = SortingLayer.NameToID("Default");
            innerLR.sortingOrder = 50;

            float rInner = Mathf.Max(0.05f, radius * Mathf.Clamp01(innerCircleScale));
            int cseg = Mathf.Max(8, innerSegments);
            innerLR.positionCount = cseg;
            for (int i = 0; i < cseg; i++)
            {
                float a = (i / (float)cseg) * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(a) * rInner, Mathf.Sin(a) * rInner, 0f);
                innerLR.SetPosition(i, p);
            }
        }

        shieldGroup.gameObject.SetActive(false); 
    }

    Vector2 GetArcAngles(ArcSide side)
    {
        switch (side)
        {
            case ArcSide.Right: return new Vector2(-Mathf.PI * 0.5f, Mathf.PI * 0.5f);
            case ArcSide.Left: return new Vector2(Mathf.PI * 0.5f, Mathf.PI * 1.5f);
            case ArcSide.Bottom: return new Vector2(Mathf.PI, Mathf.PI * 2f);
            case ArcSide.Top:
            default: return new Vector2(0f, Mathf.PI);
        }
    }
}
