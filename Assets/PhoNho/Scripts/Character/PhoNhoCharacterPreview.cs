using UnityEngine;

namespace PhoNho.Character
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PhoNhoCharacterPreview : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Movement & Preview")]
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float patrolDistance = 2.5f;
        [SerializeField] private bool autoPatrol = true;

        private Vector3 _startPosition;
        private float _patrolTimer;
        private int _direction = 1;
        private bool _isMoving;

        private static readonly int ParamIsMoving = Animator.StringToHash("IsMoving");
        private static readonly int ParamSpeed = Animator.StringToHash("Speed");

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
            _startPosition = transform.position;
        }

        private void Update()
        {
            if (autoPatrol)
            {
                UpdateAutoPatrol();
            }
            else
            {
                UpdateManualInput();
            }
        }

        private void UpdateAutoPatrol()
        {
            _patrolTimer += Time.deltaTime;
            // 3s walk, 1.5s idle pause
            float cycle = _patrolTimer % 4.5f;

            if (cycle < 3.0f)
            {
                _isMoving = true;
                float currentOffset = transform.position.x - _startPosition.x;
                if (_direction > 0 && currentOffset >= patrolDistance)
                {
                    _direction = -1;
                }
                else if (_direction < 0 && currentOffset <= -patrolDistance)
                {
                    _direction = 1;
                }

                transform.position += new Vector3(_direction * moveSpeed * Time.deltaTime, 0f, 0f);
                SetFacingDirection(_direction);
            }
            else
            {
                _isMoving = false;
                // Khi dừng lại, giữ nguyên hướng nhìn cuối (không đổi flipX)
            }

            ApplyAnimationState();
        }

        private void UpdateManualInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            _isMoving = Mathf.Abs(horizontal) > 0.01f;

            if (_isMoving)
            {
                int dir = horizontal > 0 ? 1 : -1;
                SetFacingDirection(dir);
                transform.position += new Vector3(horizontal * moveSpeed * Time.deltaTime, 0f, 0f);
            }

            ApplyAnimationState();
        }

        private void SetFacingDirection(int dir)
        {
            // Đi trái dùng flipX = true, đi phải dùng flipX = false
            if (dir < 0)
            {
                spriteRenderer.flipX = true;
            }
            else if (dir > 0)
            {
                spriteRenderer.flipX = false;
            }
        }

        private void ApplyAnimationState()
        {
            if (animator != null)
            {
                animator.SetBool(ParamIsMoving, _isMoving);
                animator.SetFloat(ParamSpeed, _isMoving ? 1f : 0f);
            }
        }
    }
}
