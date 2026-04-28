using UnityEngine;

public class DeathPlayerStateAnim : StatesAnimsAbstract
{
    public DeathPlayerStateAnim(Animator animPlayer)
    {
        ActiveAnimation("stateAnim", 6, ref animPlayer);
    }
}