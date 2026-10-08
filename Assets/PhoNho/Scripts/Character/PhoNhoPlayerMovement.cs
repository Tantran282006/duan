using UnityEngine;

namespace PhoNho.Character
{
    /// <summary>
    /// Physics-based 2D movement controller for the Player.
    /// Handles horizontal movement with keyboard input (A/D and Arrow keys),
    /// maintains vertical gravity-driven velocity, and freezes rotation.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class PhoNhoPlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Horizontal movement speed in units per second.")]
        [SerializeField] private float _moveSpeed = 3.5f;

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        [Header("Input Control")]
        [Tooltip("When true, reads Unity input system (Horizontal axis). Set false to drive via SetMoveInput.")]
        [SerializeField] private bool _useInputManager = true;

        private float _horizontalInput;
        private bool _isMoving;
        private bool _isGrounded;

        private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int ParamSpeed = Animator.StringToHash("Speed");

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        public float HorizontalInput => _horizontalInput;
        public bool IsMoving => _isMoving;
        public bool IsGrounded => _isGrounded;
        public Rigidbody2D Rigidbody => _rigidbody2D;

        public void SetMoveInput(float input)
        {
            _horizontalInput = Mathf.Clamp(input, -1f, 1f);
            UpdateFacingAndAnimation();
        }

        public void SetUseInputManager(bool enable)
        {
            _useInputManager = enable;
        }

        private void Reset()
        {
            ResolveComponents();
        }

        private void Awake()
        {
            ResolveComponents();
            ConfigurePhysics();
        }

        private void ResolveComponents()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }

            if (_collider2D == null)
            {
                _collider2D = GetComponent<Collider2D>();
            }

            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        private void ConfigurePhysics()
        {
            if (_rigidbody2D != null)
            {
                // Lock Z rotation so player remains upright
                _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                if (_rigidbody2D.gravityScale <= 0.001f)
                {
                    _rigidbody2D.gravityScale = 1.5f;
                }
            }
        }

        private void Update()
        {
            if (_useInputManager)
            {
                _horizontalInput = Input.GetAxisRaw("Horizontal");
                UpdateFacingAndAnimation();
            }
        }

        private void FixedUpdate()
        {
            ApplyHorizontalMovement();
            CheckGroundStatus();
        }

        private void UpdateFacingAndAnimation()
        {
            _isMoving = Mathf.Abs(_horizontalInput) > 0.01f;

            if (_spriteRenderer != null)
            {
                // Moving left flips sprite, moving right restores original
                if (_horizontalInput < -0.01f)
                {
                    _spriteRenderer.flipX = true;
                }
                else if (_horizontalInput > 0.01f)
                {
                    _spriteRenderer.flipX = false;
                }
            }

            if (_animator != null)
            {
                _animator.SetBool(ParamIsMoving, _isMoving);
                _animator.SetFloat(ParamSpeed, _isMoving ? 1f : 0f);
            }
        }

        private void ApplyHorizontalMovement()
        {
            if (_rigidbody2D == null)
            {
                return;
            }

#if UNITY_6000_0_OR_NEWER
            Vector2 currentVelocity = _rigidbody2D.linearVelocity;
            currentVelocity.x = _horizontalInput * _moveSpeed;
            _rigidbody2D.linearVelocity = currentVelocity;
#else
            Vector2 currentVelocity = _rigidbody2D.velocity;
            currentVelocity.x = _horizontalInput * _moveSpeed;
            _rigidbody2D.velocity = currentVelocity;
#endif
        }

        private void CheckGroundStatus()
        {
            if (_collider2D == null)
            {
                return;
            }

            // Raycast down slightly from collider bottom bounds
            Bounds bounds = _collider2D.bounds;
            Vector2 rayOrigin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
            float rayDistance = 0.15f;

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance);
            _isGrounded = hit.collider != null && hit.collider != _collider2D;
        }
    }
}
