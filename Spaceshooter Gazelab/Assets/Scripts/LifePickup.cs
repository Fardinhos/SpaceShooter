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

public class LifePickup : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Vector2 moveDir = Vector2.left;

    [Header("Pickup")]
    public string playerTag = "Player";
    public int lifeAmount = 1;

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (!col) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        rb = GetComponent<Rigidbody2D>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;
    }

    void Update()
    {
        transform.position += (Vector3)(moveDir.normalized * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || !other.CompareTag(playerTag)) return;

        other.gameObject.SendMessage("AddLife", lifeAmount, SendMessageOptions.DontRequireReceiver);

        var dir = FindObjectOfType<TutorialDirector>();
        if (dir) dir.NotifyLifeCollected();

        Destroy(gameObject);
    }
}