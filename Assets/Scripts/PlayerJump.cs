using UnityEngine;

public class PlayerJump : MonoBehaviour
{
private PlayerController playerController;

[Header ("Variables Salto")]
public float jumpForce;
public float groundRadius;

public float groundCheckDistance;
public LayerMask groundMask;
private bool isGrounded;

[Header ("Coyote Time")]
public float coyoteTime = 0.5f;
private float coyoteCounter = 0f;
private bool hasJumped= false; //Variable que verifica si el jugador termino de saltar


[Header ("Buffer Jump")]
public float bufferJumpTime = 0.5f;
public float bufferJumpCounter = 0f;



//GETTERS
public bool IsGrounded {
    get {return isGrounded;}}


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void OnUpdate()
    {
        CheckGround();
        JumpUpdates();
       
    }




//Metodo para verificar suelo (si el player esta en contacto con un objeto o layer de tipo suelo)
public void CheckGround()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, groundRadius, Vector2.down, groundCheckDistance, groundMask );
        
        //verificar si el circle cast esta en contacto con el layer ground
        if (hit.collider != null)
        {
            isGrounded = true;
        } else
        {
            isGrounded = false;
        }
    }

    public void JumpHold()
    {
        
        if ((isGrounded || coyoteCounter > 0) && !hasJumped) //Realizamos un salto
        {
            playerController.rb.linearVelocity = new Vector2 (playerController.rb.linearVelocity.x, jumpForce);
            
            // 👇 MÍNIMO DE SALTO
        if (playerController.rb.linearVelocity.y < 2f)
        {
        playerController.rb.linearVelocity = new Vector2(
            playerController.rb.linearVelocity.x,
            2f
            );
        }

            
            //ajustar la gravedad normal para el personaje 
            playerController.rb.gravityScale = playerController.normalGravity;
            coyoteCounter = 0;
            hasJumped = true;
        }
        else //no se ha realizado el salto
        {
            bufferJumpCounter = bufferJumpTime; //volvemos a dar margen de tiempo para el buffer jump
        }
    }

//Se encarga de actualizar las mejoras del salt; gravedad dinamica, coyote time y buffer jump
public void JumpUpdates()
    {


        //Este if else verifica el coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            hasJumped = false; //Resetear variable hasJumped

        }else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        //Este if verifica el buffer Jump
        if (bufferJumpCounter > 0)
        {
            coyoteCounter -= Time.fixedDeltaTime;
            //si tocamos el suelo y hay un buffer > 0 (buffer activo) = salto automatico
            if(isGrounded)
            {
            playerController.rb.linearVelocity = new Vector2 (playerController.rb.linearVelocity.x, jumpForce);
            //ajustar la gravedad normal para el personaje 
            playerController.rb.gravityScale = playerController.normalGravity;
            coyoteCounter = 0;
            bufferJumpCounter = 0;
            }

        }

        //Este if else actualiza la gravedad dinamica del personaje
        if (isGrounded)
        {
            playerController.rb.gravityScale = playerController.normalGravity;
        } else if (playerController.rb.linearVelocity.y < -0.1f) //En caso de que estemos en caida
        {
            playerController.rb.gravityScale = playerController.fallGravity;
        }
    }

public void JumpRelease()
{
    hasJumped = true;
}

    private void OnDrawGizmos()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.green;
        } else
        {
            Gizmos.color = Color.red;
        }


        Vector3 checkPosition = transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(checkPosition, groundRadius);
    }

}


