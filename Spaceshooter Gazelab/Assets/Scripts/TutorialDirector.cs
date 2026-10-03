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

public class TutorialDirector : MonoBehaviour
{
    [Header("Refs")]
    public DialogBox dialog;

    [Tooltip("Wird automatisch via Tag 'Player' gesucht, falls leer.")]
    public Transform player;

    [Header("Asteroid")]
    public GameObject asteroidPrefab;
    public Vector2 asteroidVelocity = new Vector2(-3f, 0f);

    [Header("Enemy")]
    public GameObject enemyPrefab;
    public float enemySpeed = 2.5f;

    [Header("Life Pickup")]
    public GameObject lifePrefab;
    public float lifeSpawnDelay = 0.6f;
    public float lifeMoveSpeed = 2f;

    [Header("Spawn Common")]
    public float outsideOffset = 0.6f;
    public float insideSideMargin = 0.2f;

    [Header("UI-Reserve (Viewport)")]
    [Range(0f, 0.5f)] public float bottomReservedViewport = 0.28f;
    [Range(0f, 0.2f)] public float topViewportMargin = 0.05f;

    [Header("Timings")]
    public float freeFlySeconds = 3f;
    public float waitVisibleTimeout = 2f;
    public float minVisibleSeconds = 0.5f;

    [Header("Texte")]
    [TextArea] public string[] introLines;
    [TextArea] public string[] shootLines;
    [TextArea] public string[] evadeLines;
    [TextArea] public string[] playerHitLines;
    [TextArea] public string[] dodgeBulletsLines;
    [TextArea] public string[] lifeSpawnLines;
    [TextArea] public string[] lifeCollectedLines;

    [Header("Flow")]
    public string mainMenuSceneName = "MainMenu";

    Coroutine evadeRoutine, playerHitRoutine, dodgeRoutine, lifeRoutine, lifeCollectedRoutine;

    bool lifeSpawnedForThisEnemy = false;

    IEnumerator Start()
    {
        if (!dialog) dialog = FindObjectOfType<DialogBox>();
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (introLines != null && introLines.Length > 0)
            yield return dialog.Show(introLines);

        yield return new WaitForSeconds(freeFlySeconds);

        yield return SpawnAsteroidThenShowShootHint();

        yield return SpawnEnemyThenShowDodgeHint();
    }

    IEnumerator SpawnAsteroidThenShowShootHint()
    {
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float yVpMin = Mathf.Clamp01(bottomReservedViewport + 0.02f);
        float yVpMax = 1f - topViewportMargin;
        float yVp = Random.Range(yVpMin, yVpMax);
        float y = cam.ViewportToWorldPoint(new Vector3(0.5f, yVp, 0f)).y;

        Vector3 spawnPos = new Vector3(cam.transform.position.x + halfW + outsideOffset, y, 0f);
        var go = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity);

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr) { sr.sortingLayerName = "Default"; sr.sortingOrder = 1000; }

        var mover = go.GetComponent<AsteroidMover2D>();
        if (mover)
        {
            mover.direction = Vector2.left;
            mover.speed = Mathf.Abs(asteroidVelocity.x);
            mover.SetBounds(CamRect(cam), outsideOffset);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) { rb.gravityScale = 0f; rb.velocity = asteroidVelocity; }
        }

        yield return WaitUntilBrieflyVisible(go, cam);

        if (shootLines != null && shootLines.Length > 0)
            yield return dialog.Show(shootLines);

        yield return WaitUntilGoneLeftOrDestroyed(go, cam, outsideOffset);
    }

    IEnumerator SpawnEnemyThenShowDodgeHint()
    {
        if (!enemyPrefab) yield break;
        lifeSpawnedForThisEnemy = false;

        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float y = player ? player.position.y : cam.transform.position.y;
        float yMin = cam.transform.position.y - halfH + insideSideMargin;
        float yMax = cam.transform.position.y + halfH - insideSideMargin;
        y = Mathf.Clamp(y, yMin, yMax);

        Vector3 spawnPos = new Vector3(cam.transform.position.x + halfW + outsideOffset, y, 0f);
        var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        var sr = enemy.GetComponentInChildren<SpriteRenderer>();
        if (sr) { sr.sortingLayerName = "Default"; sr.sortingOrder = 1000; }

        var ai = enemy.GetComponentInChildren<EnemyShipAI>();
        if (ai != null)
        {
            ai.InitMoveLeft(enemySpeed);
        }
        else
        {
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb) { rb.gravityScale = 0f; rb.velocity = Vector2.left * Mathf.Abs(enemySpeed); }
            else StartCoroutine(SimpleMoveLeft(enemy, enemySpeed));
        }

        yield return WaitUntilBrieflyVisible(enemy, cam);

        if (dodgeBulletsLines != null && dodgeBulletsLines.Length > 0 && dodgeRoutine == null)
            dodgeRoutine = StartCoroutine(ShowDodgeBullets());

        Vector3 lastPos = enemy ? enemy.transform.position : spawnPos;
        float leftEdge = cam.transform.position.x - (cam.orthographicSize * cam.aspect) - outsideOffset;

        while (enemy && enemy.transform.position.x > leftEdge)
        {
            lastPos = enemy.transform.position;
            if (lifeSpawnedForThisEnemy) yield break;
            yield return null;
        }

        if (!lifeSpawnedForThisEnemy)
        {
            if (enemy) Destroy(enemy);
            StartCoroutine(SpawnLifeAfterDelay(lastPos));
        }
    }


    public void NotifyAsteroidHit()
    {
        if (evadeRoutine == null && evadeLines != null && evadeLines.Length > 0)
            evadeRoutine = StartCoroutine(ShowEvade());
    }

    public void NotifyPlayerHit()
    {
        if (playerHitRoutine == null && playerHitLines != null && playerHitLines.Length > 0)
            playerHitRoutine = StartCoroutine(ShowPlayerHit());
    }

    public void NotifyEnemyDestroyed(Vector3 atPosition)
    {
        if (lifeSpawnedForThisEnemy) return;
        StartCoroutine(SpawnLifeAfterDelay(atPosition));
    }

    public void NotifyLifeCollected()
    {
        if (lifeCollectedRoutine == null &&
            lifeCollectedLines != null && lifeCollectedLines.Length > 0)
        {
            lifeCollectedRoutine = StartCoroutine(ShowLifeCollected());
        }
    }

    IEnumerator SpawnLifeAfterDelay(Vector3 worldPos)
    {
        lifeSpawnedForThisEnemy = true;
        yield return new WaitForSeconds(lifeSpawnDelay);

        var cam = Camera.main;

        float y = ClampYToVisibleArea(worldPos.y, cam);

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = cam.transform.position.x + halfW + outsideOffset;

        Vector3 spawnPos = new Vector3(x, y, 0f);

        if (lifePrefab)
        {
            var life = Instantiate(lifePrefab, spawnPos, Quaternion.identity);

            var sr = life.GetComponentInChildren<SpriteRenderer>();
            if (sr) { sr.sortingLayerName = "Default"; sr.sortingOrder = 1000; }

            var col = life.GetComponent<Collider2D>();
            if (!col) col = life.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            var rb = life.GetComponent<Rigidbody2D>();
            if (!rb) rb = life.AddComponent<Rigidbody2D>();
            rb.isKinematic = true; rb.gravityScale = 0f;

            var lp = life.GetComponent<LifePickup>();
            if (!lp) lp = life.gameObject.AddComponent<LifePickup>();
            lp.moveDir = Vector2.left;
            lp.moveSpeed = lifeMoveSpeed;
        }

        if (lifeSpawnLines != null && lifeSpawnLines.Length > 0 && lifeRoutine == null)
            lifeRoutine = StartCoroutine(ShowLifeSpawn());
    }


    IEnumerator ShowEvade() { yield return dialog.Show(evadeLines); evadeRoutine = null; }
    IEnumerator ShowPlayerHit() { yield return dialog.Show(playerHitLines); playerHitRoutine = null; }
    IEnumerator ShowDodgeBullets() { yield return dialog.Show(dodgeBulletsLines); dodgeRoutine = null; }
    IEnumerator ShowLifeSpawn() { yield return dialog.Show(lifeSpawnLines); lifeRoutine = null; }

    IEnumerator ShowLifeCollected()
    {
        yield return dialog.Show(lifeCollectedLines);
        lifeCollectedRoutine = null;

        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    Rect CamRect(Camera cam)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        return new Rect(cam.transform.position.x - halfW,
                        cam.transform.position.y - halfH,
                        2f * halfW, 2f * halfH);
    }

    float ClampYToVisibleArea(float worldY, Camera cam)
    {
        float yMinVp = Mathf.Clamp01(bottomReservedViewport + 0.06f);
        float yMaxVp = 1f - topViewportMargin;

        float yMin = cam.ViewportToWorldPoint(new Vector3(0f, yMinVp, 0f)).y;
        float yMax = cam.ViewportToWorldPoint(new Vector3(0f, yMaxVp, 0f)).y;

        return Mathf.Clamp(worldY, yMin, yMax);
    }

    IEnumerator WaitUntilBrieflyVisible(GameObject go, Camera cam)
    {
        float t = 0f, visibleFor = 0f;
        while (go && t < waitVisibleTimeout)
        {
            Vector3 v = cam.WorldToViewportPoint(go.transform.position);
            bool onScreen = v.z > 0f && v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f;

            if (onScreen)
            {
                visibleFor += Time.deltaTime;
                if (visibleFor >= minVisibleSeconds) yield break;
            }
            else visibleFor = 0f;

            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator WaitUntilGoneLeftOrDestroyed(GameObject go, Camera cam, float margin)
    {
        if (!go) yield break;

        float halfW = cam.orthographicSize * cam.aspect;
        float leftEdge = cam.transform.position.x - halfW - margin;

        while (go && go.transform.position.x > leftEdge)
            yield return null;
    }

    IEnumerator SimpleMoveLeft(GameObject go, float speed)
    {
        while (go)
        {
            go.transform.position += Vector3.left * (speed * Time.deltaTime);
            yield return null;
        }
    }
}