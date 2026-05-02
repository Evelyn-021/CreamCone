using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
[ HideInInspector]  public Rigidbody2D rb;
    
[ HideInInspector] public Animator animPlayer;
[HideInInspector] public bool isDead;
[HideInInspector] public bool isWaterBubbleRiding;
[HideInInspector] public bool isRespawning;

    public Controles controles;

    [Header("Variables Movimiento")]

    //Variables movimiento 
    public float speed;


    [Header("Variables de salto")]
    public float normalGravity = 2f;
    public float fallGravity = 4f;
    

    //MECANICAS
    [ HideInInspector] public UpdateAnimsPlayer updateAnimsPlayer;
    [ HideInInspector] public PlayerMovement movement;
    [ HideInInspector] public PlayerJump jump;

    //Para el ataque 
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public PlayerAttack attack;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animPlayer = GetComponentInChildren<Animator>();
        controles = new Controles();
        //CONECTAR MECANICAS
        updateAnimsPlayer = GetComponent<UpdateAnimsPlayer>();
        movement = GetComponent<PlayerMovement>();
        jump = GetComponent<PlayerJump>();
        //Inicializar Ataque
        attack = GetComponent<PlayerAttack>();

    }

    void FixedUpdate()
    {
        if (isWaterBubbleRiding || isRespawning || isDead) return;

        movement.Move();//Movimiento
        jump.OnUpdate(); //salto
    }

    private void Update()
    {
        if (isWaterBubbleRiding || isRespawning) return;

        updateAnimsPlayer.UpdateAnimation();
    }
    private void OnEnable() //Se llaman cada vez que se activa el script 
    {
        if (controles == null)
        {
            controles = new Controles();
        }

        controles.Enable();
        controles.Player.Jump.performed += OnJump;
        controles.Player.Jump.canceled += OnJumpRelease;
        controles.Player.Attack.performed += OnAttack;
    }

  

    private void OnDisable() //cada vez que se desactiva el script
    {
        if (controles == null) return;

        controles.Disable();
        controles.Player.Jump.performed -= OnJump;
        controles.Player.Jump.canceled -= OnJumpRelease;
        controles.Player.Attack.performed -= OnAttack;
    }


    private void OnAttack(InputAction.CallbackContext context)
{
    if (isRespawning || isDead) return;

    attack.StartAttack();
}

      private void OnJump (InputAction.CallbackContext context)
    {
        if (isRespawning || isDead) return;

        jump.JumpHold();
    }
private void OnJumpRelease (InputAction.CallbackContext context)
    {
        if (isRespawning || isDead) return;

        jump.JumpRelease();
    }

    public void SetWaterBubbleRide(bool isRiding)
    {
        isWaterBubbleRiding = isRiding;

        if (rb == null) return;

        rb.gravityScale = isRiding ? 0f : normalGravity;
        rb.linearVelocity = Vector2.zero;
    }


    public void Die()
{
    if (isDead) return;

    isDead = true;
    rb.linearVelocity = Vector2.zero;

    new DeathPlayerStateAnim(animPlayer);
}

public void SetRespawning(bool respawning)
{
    isRespawning = respawning;

    if (rb != null)
    {
        rb.linearVelocity = Vector2.zero;
    }
}
}
