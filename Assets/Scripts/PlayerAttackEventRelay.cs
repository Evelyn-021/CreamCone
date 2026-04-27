using UnityEngine;

public class PlayerAttackEventRelay : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public void SpawnIceBubble()
    {
        if (playerAttack != null)
            playerAttack.SpawnIceBubble();
    }

    public void EndAttack()
    {
        if (playerAttack != null)
            playerAttack.EndAttack();
    }
}