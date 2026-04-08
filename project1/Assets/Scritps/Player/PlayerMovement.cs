using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!inputEnabled)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (!inputEnabled) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    public void SetMovementEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            movement = Vector2.zero;
            if (rb != null)
                rb.velocity = Vector2.zero;
        }
    }

    public bool IsMovementEnabled() => inputEnabled;

    public Vector2 Movement => movement;
}