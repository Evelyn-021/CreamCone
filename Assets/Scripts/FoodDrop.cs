using UnityEngine;

public class FoodDrop : MonoBehaviour
{
    public int points = 100;
    public float fallSpeedLimit = -2f;
    public float descendSpeed = 0f;
    public float autoDestroyY = -999f;
    private Rigidbody2D rb;
    private bool collected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (descendSpeed > 0f)
        {
            rb.linearVelocity = new Vector2(0f, -descendSpeed);
        }

        if (rb.linearVelocity.y < fallSpeedLimit)
        {
            rb.linearVelocity = new Vector2(0f, fallSpeedLimit);
        }

        if (!collected && transform.position.y <= autoDestroyY)
        {
            collected = true;

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitCollected();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collected) return;
            collected = true;

            if (BitmapScoreUI.Instance != null)
            {
                BitmapScoreUI.Instance.AddScore(points);
            }

            if (LevelGoalManager.Instance != null)
            {
                LevelGoalManager.Instance.FruitCollected();
            }

            Destroy(gameObject);
        }
    }
}
