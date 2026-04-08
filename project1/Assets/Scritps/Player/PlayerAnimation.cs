using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public float frameRate = 0.15f;

    private SpriteRenderer sr;
    private Vector2 movement;
    private Vector2 lastDirection;

    private float timer;
    private int frameIndex;

    private PlayerMovement playerMovement;

    [Header("Walk Animations")]
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;

    [Header("Idle Sprites")]
    public Sprite idleDown;
    public Sprite idleUp;
    public Sprite idleLeft;
    public Sprite idleRight;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        
        movement = playerMovement != null ? playerMovement.Movement : Vector2.zero;

        bool isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            lastDirection = movement;

            timer += Time.deltaTime;

            if (timer >= frameRate)
            {
                timer = 0f;
                frameIndex++;
            }

            UpdateWalkAnimation();
        }
        else
        {
            frameIndex = 0;
            UpdateIdleSprite();
        }
    }

    void UpdateWalkAnimation()
    {
        Sprite[] currentAnim = walkDown;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            currentAnim = movement.x > 0 ? walkRight : walkLeft;
        }
        else
        {
            currentAnim = movement.y > 0 ? walkUp : walkDown;
        }

        if (currentAnim == null || currentAnim.Length == 0)
            return;

        frameIndex %= currentAnim.Length;
        sr.sprite = currentAnim[frameIndex];
    }

    void UpdateIdleSprite()
    {
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            sr.sprite = lastDirection.x > 0 ? idleRight : idleLeft;
        }
        else
        {
            sr.sprite = lastDirection.y > 0 ? idleUp : idleDown;
        }
    }
}