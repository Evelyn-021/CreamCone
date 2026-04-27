using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerController playerController;

    public GameObject projectilePrefab;
    public Transform attackPoint;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void StartAttack()
    {
        if (playerController.isAttacking) return;

        playerController.isAttacking = true;
    }

    public void SpawnIceBubble()
    {
        GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);

        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        proj.GetComponent<IceBubbleProjectile>().SetDirection(dir);
    }

    public void EndAttack()
    {
        playerController.isAttacking = false;
    }
}