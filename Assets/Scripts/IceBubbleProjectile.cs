using UnityEngine;

public class IceBubbleProjectile : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 direction = Vector2.right;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, 2f); // por si no choca con nada
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
        return;

    if (other.CompareTag("Enemy"))
    {
        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy != null)
        {
            enemy.TrapEnemy();
        }

        Destroy(gameObject);
        return;
    }

    Destroy(gameObject);
}
}