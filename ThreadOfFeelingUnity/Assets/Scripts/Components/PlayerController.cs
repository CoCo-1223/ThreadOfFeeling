using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private Rigidbody2D _rigidbody;
    private float speed = 3;
    private Vector3 move;
    //public NPC dialog;
    GameObject scanObject;

    private bool isStopped = false;
    private Vector2 lastMoveDir = Vector2.down;

    void Start() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update() {
        if (isStopped) move = Vector3.zero;
        // 이동 입력 가져오기 8방향 정규화된 벡터
        else move = InputManager.Instance.GetMoveInput(); 

        if (move.magnitude > 0) lastMoveDir = move.normalized;

        // 마지막 바라보는 방향 기준으로 좌우 반전
        if (move.x < 0) _spriteRenderer.flipX = true;
        else if (move.x > 0) _spriteRenderer.flipX = false;

        // 플레이어 애니메이션
        if (move.magnitude > 0) _animator.SetTrigger("Move");
        else _animator.SetTrigger("Stop");

        // NPC 상호작용 Scan Object
        if (InputManager.Instance.GetInteractionKeyDown() && scanObject != null) {
            NPC targetNPC = scanObject.GetComponent<NPC>();
            if (targetNPC != null) {
                targetNPC.Action(scanObject);
                if (targetNPC.getIsAction()) {
                    StopMovement();
                }
                else {
                    ResumeMovement();
                }
            }
        }
    }

    private void FixedUpdate() {
        // 이동
        if (!isStopped) {
            Vector3 nextPosition = _rigidbody.position + (Vector2)(move * speed * Time.deltaTime);
            _rigidbody.MovePosition(nextPosition);
        }

        // Ray 시각화
        //Vector3 facingDir = InputManager.Instance.GetFacingDirection();
        Vector3 facingDir = lastMoveDir;
        Debug.DrawRay(transform.position, facingDir * 0.7f, Color.green);
        RaycastHit2D rayHit = Physics2D.Raycast(transform.position, facingDir, 0.7f, LayerMask.GetMask("Object"));

        if (rayHit.collider != null) {
            scanObject = rayHit.collider.gameObject;
        }
        else scanObject = null;
    }

    public void StopMovement() {
        isStopped = true;
        _animator.SetTrigger("Stop"); // 멈출 때 애니메이션도 정지
    }

    public void ResumeMovement() { // 이동 재개 함수 추가
        isStopped = false;
    }
}