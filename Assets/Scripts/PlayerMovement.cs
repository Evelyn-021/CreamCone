using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController playerController;

    private bool isFacingRight = true;
    private bool isMoving; //variable bool que verifica si el jugador se mueve
   

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        
    }
    public void Move()
    {
        Vector2 move = playerController.controles.Player.Move.ReadValue<Vector2>();
        playerController.rb.linearVelocity = new Vector2(move.x * playerController.speed, playerController.rb.linearVelocity.y);
        //dentro del bool isMoving guardamos si el player se mueve o no 
        isMoving = move.x != 0;

//LLAMADO A LA MECANICA DE FLIP
    if (move.x > 0 && !isFacingRight)
        {
           Flip();
        } else if (move.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        if (isFacingRight)
        {
            transform.localScale = new Vector2(1, 1);
        }else
        {
            transform.localScale = new Vector2(-1, 1);
        }
    }

    public bool IsMoving //Getter de la variable isMoving que nos informa si nos estamos moviendo
    {
        get { return isMoving; }
    }
}
