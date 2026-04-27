using UnityEngine;

public class AttackPlayerStateAnim : StatesAnimsAbstract
{
    public AttackPlayerStateAnim(Animator animPlayer)
    {
        ActiveAnimation("stateAnim", 5, ref animPlayer);
    }
}