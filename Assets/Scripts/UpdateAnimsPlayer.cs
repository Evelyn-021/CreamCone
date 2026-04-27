using UnityEngine;

public class UpdateAnimsPlayer : MonoBehaviour
{

    private PlayerController playerController;
     private AnimationManager animationManager;
    

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        animationManager = new AnimationManager();
        
    }


    public void UpdateAnimation()
    { 

        //Actualizar animaciones de ataque
        if (playerController.isAttacking)
        {
            animationManager.setState(new AttackPlayerStateAnim(playerController.animPlayer));
            return;
        }
                
        //actualizar animaciones de salto

        if (!playerController.jump.IsGrounded) //si el jugador no esta tocando el suelo
        {
            //Estamos en el aire
            if (playerController.rb.linearVelocity.y > 0.1)
            {
            //subiendo
            animationManager.setState(new JumpStartPlayerStateAnim(playerController.animPlayer));
            } else if (playerController.rb.linearVelocity.y < -0.1)
            {
                //cayendo
            animationManager.setState(new JumpEndPlayerStateAnim(playerController.animPlayer));

            }
            return; //evitar que otras animaciones se pisen
        }
        if (playerController.movement.IsMoving) //si el jugador se esta moviendo
        {
            animationManager.setState(new RunPlayerStateAnim(playerController.animPlayer));
        } else
        {//si el jugador NO se esta moviendo
            animationManager.setState(new IdlePlayerStateAnim(playerController.animPlayer));
        }
    }
}
