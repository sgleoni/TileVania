using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Vector2 moveInput;
    Rigidbody2D playerRigidbody;
    Animator playerAnimator;
    CapsuleCollider2D playerBodyCollider;
    BoxCollider2D playerFeetCollider;
    float gravityScaleTemp = 0;
    bool isAlive;

    [Header("Movement")]
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 7f;

    [Header("Death")]
    [SerializeField] Vector2 deathKick = new(10f, 10f);

    [Header("Attack")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;


    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        playerFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleTemp = playerRigidbody.gravityScale;
        isAlive = true;
    }

    void Update()
    {
        Die();

        if (isAlive)
        {
            Run();
            FlipSprite();
            Climb();
        }
    }

    private void Die()
    {
        if (isAlive && playerBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            isAlive = false;
            playerAnimator.SetTrigger("death");
            playerRigidbody.velocity = deathKick;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    void OnFire(InputValue value)
    {
        if (isAlive)
        {
            Instantiate(bullet, gun.position, transform.rotation);
        }
    }

    void OnMove(InputValue value)
    {
        if (isAlive)
        {
            moveInput = value.Get<Vector2>();
        }
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && PlayerIsOnGround() && isAlive)
        {
            playerRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }
    }

    void Run()
    {
        playerRigidbody.velocity = new(moveInput.x * runSpeed, playerRigidbody.velocity.y);

        playerAnimator.SetBool("isRunning", PlayerHasHorizontalSpeed());
    }

    void Climb()
    {
        if (PlayerIsOnLadders())
        {
            playerRigidbody.velocity = new(playerRigidbody.velocity.x, moveInput.y * climbSpeed);
            playerRigidbody.gravityScale = 0;
        }
        else
        {
            playerRigidbody.gravityScale = gravityScaleTemp;
        }

        playerAnimator.SetBool("isClimbing", PlayerHasVerticalSpeed() && PlayerIsOnLadders());

    }

    private bool PlayerIsOnGround()
    {
        return playerFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private bool PlayerIsOnLadders()
    {
        return playerFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ladders"));
    }

    private void FlipSprite()
    {
        if (PlayerHasHorizontalSpeed())
        {
            transform.localScale = new(Mathf.Sign(playerRigidbody.velocity.x), 1f);
        }
    }

    private bool PlayerHasVerticalSpeed()
    {
        return Mathf.Abs(playerRigidbody.velocity.y) > Mathf.Epsilon;
    }

    private bool PlayerHasHorizontalSpeed()
    {
        return Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
    }
}
