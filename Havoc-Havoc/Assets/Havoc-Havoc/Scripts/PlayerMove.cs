using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f; // disable gravity for top-down movement
    }

    void Update()
    {
        // Get input for both axes (WASD / arrow keys)
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Set animator parameter
        animator.SetBool("isWalking", moveInput.magnitude > 0.01f);
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        // Move the character
        rb.velocity = moveInput * moveSpeed;
    }
}