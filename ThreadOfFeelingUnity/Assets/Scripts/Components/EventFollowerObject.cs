using UnityEngine;

namespace Components
{
    [RequireComponent(typeof(Rigidbody2D))] 
    public class EventFollowerObject : InteractableObject
    {
        [Header("Follow Settings")]
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private float moveSpeed = 2.5f;
        
        [Tooltip("이 거리보다 가까워지면 멈춥니다.")]
        [SerializeField] private float stopDistance = 1.5f;
        
        [Tooltip("멈춘 상태에서 이 거리만큼 더 멀어져야 다시 움직입니다 (떨림 방지용 버퍼).")]
        [SerializeField] private float moveBuffer = 0.5f;

        [Header("Condition")]
        [SerializeField] private bool followAfterTalk = true;

        [Header("Animation")]
        [SerializeField] private string walkBoolParam = "isWalking";
        [SerializeField] private bool flipXOnLeft = true;

        private Transform targetTransform;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private Rigidbody2D rb;
        
        private bool isFollowing = false;
        private bool isMoving = false;

        protected override void Start()
        {
            base.Start();

            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic; // 코드로만 이동
                //rb.gravityScale = 0f;
                //rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 부드러운 보간
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
            }
        }

        protected override void EndInteraction()
        {
            base.EndInteraction();
            if (followAfterTalk) isFollowing = true;
        }

        private void FixedUpdate()
        {
            if (!isFollowing || isAction || targetTransform == null)
            {
                StopMovement();
                return;
            }

            float distance = Vector2.Distance(rb.position, targetTransform.position);
            
            float activeThreshold = isMoving ? stopDistance : (stopDistance + moveBuffer);

            if (distance > activeThreshold)
            {
                isMoving = true;
                MoveTowardsTarget();
            }
            else
            {
                isMoving = false;
                StopMovement();
            }
        }

        private void MoveTowardsTarget()
        {
            Vector2 nextPosition = Vector2.MoveTowards(rb.position, targetTransform.position, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPosition);

            // 방향 전환 (스프라이트)
            if (spriteRenderer != null)
            {
                float xDifference = targetTransform.position.x - transform.position.x;
                if (Mathf.Abs(xDifference) > 0.1f) 
                {
                    spriteRenderer.flipX = (xDifference > 0) ? !flipXOnLeft : flipXOnLeft;
                }
            }

            if (animator != null) animator.SetBool(walkBoolParam, true);
        }

        private void StopMovement()
        {
            if (animator != null) animator.SetBool(walkBoolParam, false);
            rb.linearVelocity = Vector2.zero; 
        }
    }
}