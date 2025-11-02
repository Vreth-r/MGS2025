using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;

    // Scene boundary variables
    public float leftBound = -7f;
    public float rightBound = 7f;
    public float bottomBound = -2f;
    public float topBound = 5f;

    // Input System generated actions
    private InputSystem_Actions inputActions;
    private bool isAttacking = false;

    void Awake()
    {
        // This new class must be generated from the Input System and matches your class name
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Attack.canceled += OnAttackCanceled;
    }

    void OnDisable()
    {
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Attack.canceled -= OnAttackCanceled;
        inputActions.Player.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable gravity for top-down movement
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Use generated InputSystem_Actions class to read movement
        movement = inputActions.Player.Move.ReadValue<Vector2>();

        if (animator != null)
            animator.SetBool("isAttacking", isAttacking);
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        // Clamp position to stay within bounds
        newPosition.x = Mathf.Clamp(newPosition.x, leftBound, rightBound);
        newPosition.y = Mathf.Clamp(newPosition.y, bottomBound, topBound);
        rb.MovePosition(newPosition);
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        isAttacking = true;
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        isAttacking = false;
    }

    // Detect collision with enemy and call TakeHit if attacking
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAttacking && collision.gameObject.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeHit();
            }
        }
    }
}
