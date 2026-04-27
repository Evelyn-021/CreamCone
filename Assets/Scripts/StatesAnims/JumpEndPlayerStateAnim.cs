using UnityEngine;

public class JumpEndPlayerStateAnim : StatesAnimsAbstract
{

public JumpEndPlayerStateAnim (Animator animPlayer)
    {
        ActiveAnimation("stateAnim", 4, ref animPlayer);
    }


}
