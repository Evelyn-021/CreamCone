using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (LifeManager.Instance != null)
            {
                LifeManager.Instance.LoseLife();
            }
        }
    }
}