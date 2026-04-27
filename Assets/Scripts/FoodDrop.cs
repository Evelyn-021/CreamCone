using UnityEngine;

public class FoodDrop : MonoBehaviour
{
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
}