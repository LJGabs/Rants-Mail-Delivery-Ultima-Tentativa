using UnityEngine;

namespace RantsMailDelivery
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    public class RunnerPlayer : MonoBehaviour
    {
        public float runSpeed = 6.1f;
        public float jumpVelocity = 11.6f;
        public float normalGravity = 3.15f;
        public float glideGravity = 0.64f;
        public float glideFallSpeed = 2.0f;
        public SpriteRenderer bodySprite;
        public SpriteRenderer leafSprite;
        public SpriteRenderer capSprite;

        private Rigidbody2D rb;
        private bool grounded;
        private bool rolling;
        private float rollUntil;
        private float jumpBufferUntil;
        private float coyoteUntil;
        private bool holdJump;
        private Vector3 originalScale;

        public bool IsRolling => rolling;
        public bool IsGliding { get; private set; }
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = normalGravity;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            originalScale = transform.localScale;
        }

        private void Update()
        {
            var director = GameDirector.Instance;
            if (director == null || !director.IsPlaying)
            {
                IsGliding = false;
                rolling = false;
                if (leafSprite != null) leafSprite.enabled = false;
                return;
            }

            bool pressedJump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetMouseButtonDown(0);
            holdJump = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetMouseButton(0);
            bool rollPressed = Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(2) || Input.mouseScrollDelta.y < -0.1f;

            // Versão touch: tocar = pular/segurar = planar; deslizar para baixo = rolar.
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began) pressedJump = true;
                holdJump |= t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                if (t.phase == TouchPhase.Moved && t.deltaPosition.y < -45f) rollPressed = true;
            }

            if (pressedJump) jumpBufferUntil = Time.time + 0.13f;
            if (rollPressed && grounded) rollUntil = Time.time + 0.73f;
            rolling = grounded && Time.time < rollUntil;
            IsGliding = director.CanGlide && holdJump && !grounded && rb.linearVelocity.y < 0.5f;
            rb.gravityScale = IsGliding ? glideGravity : normalGravity;
            if (leafSprite != null) leafSprite.enabled = IsGliding;

            if (bodySprite != null)
            {
                bodySprite.color = director.IsTired ? new Color(1f, .68f, .65f) : Color.white;
                bodySprite.transform.localScale = rolling ? new Vector3(1.22f, .68f, 1f) : Vector3.one;
            }
            if (capSprite != null) capSprite.color = director.BlueHatEquipped ? new Color(.3f, .75f, 1f) : new Color(1f, .79f, .35f);
        }

        private void FixedUpdate()
        {
            var director = GameDirector.Instance;
            if (rb == null || director == null) return;
            grounded = CheckGround();
            if (grounded) coyoteUntil = Time.time + 0.10f;
            if (!director.IsPlaying)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                return;
            }
            if (Time.time <= jumpBufferUntil && Time.time <= coyoteUntil)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
                jumpBufferUntil = -1f;
                coyoteUntil = -1f;
                grounded = false;
            }
            CurrentSpeed = director.IsTired ? runSpeed * .58f : runSpeed;
            float targetY = rb.linearVelocity.y;
            if (IsGliding && targetY < -glideFallSpeed) targetY = -glideFallSpeed;
            rb.linearVelocity = new Vector2(CurrentSpeed, targetY);
        }

        private bool CheckGround()
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + Vector2.down * .57f, new Vector2(.48f, .20f), 0f);
            foreach (var col in hits)
                if (col != null && !col.isTrigger && col.GetComponent<GroundSegment>() != null) return true;
            return false;
        }

        public void ResetTo(float x)
        {
            rb.position = new Vector2(x, -.80f);
            transform.position = new Vector3(x, -.80f, 0);
            rb.linearVelocity = Vector2.zero;
            rolling = false;
            IsGliding = false;
            jumpBufferUntil = -1f;
            coyoteUntil = -1f;
            rollUntil = -1f;
            rb.gravityScale = normalGravity;
            if (leafSprite != null) leafSprite.enabled = false;
        }
    }

}
