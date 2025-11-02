using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public int maxHits = 3;
    private int hitCount = 0;

    // Spawn position and prefab reference
    public Vector2 spawnPosition = new Vector2(5.4f, -2.02f);
    public GameObject enemyPrefab;

    // Scene boundary variables
    public float leftBound = -7f;
    public float rightBound = 7f;
    public float bottomBound = -2f;
    public float topBound = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        // Face left
        transform.localScale = new Vector3(-1.21f, 1.21f, 1.21f);

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true; // Prevent rotation
        }
    }

    void Update()
    {
        Vector2 newPosition;
        if (rb != null)
        {
            newPosition = rb.position + Vector2.left * moveSpeed * Time.deltaTime;
            // Clamp position to stay within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
            newPosition.y = Mathf.Clamp(newPosition.y, bottomBound, topBound);
            rb.MovePosition(newPosition);
        }
        else
        {
            newPosition = transform.position + Vector3.left * moveSpeed * Time.deltaTime;
            // Clamp position to stay within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
            newPosition.y = Mathf.Clamp(newPosition.y, bottomBound, topBound);
            transform.position = newPosition;
        }
    }

    public void TakeHit()
    {
        Debug.Log("Enemy hit!");
        hitCount++;
        if (hitCount >= maxHits)
        {
            Debug.Log("Enemy defeated! Respawning...");
            Respawn();
        }
    }

    private void Respawn()
    {
        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Destroy(gameObject);
    }
}
