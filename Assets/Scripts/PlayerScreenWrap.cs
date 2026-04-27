using UnityEngine;

public class PlayerScreenWrap : MonoBehaviour
{
    public float bottomLimit = -6f;
    public float topSpawnY = 6f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (transform.position.y < bottomLimit)
        {
            WrapToTop();
        }
    }

    private void WrapToTop()
    {
        Vector3 newPosition = transform.position;
        newPosition.y = topSpawnY;
        transform.position = newPosition;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    }
}