using UnityEngine;

public class FoodDrop : MonoBehaviour
{

    public int points = 100;
    public float fallSpeedLimit = -2f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity.y < fallSpeedLimit)
        {
            rb.linearVelocity = new Vector2(0f, fallSpeedLimit);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
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