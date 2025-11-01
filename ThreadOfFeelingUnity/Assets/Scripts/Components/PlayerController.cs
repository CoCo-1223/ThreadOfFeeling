using UnityEngine;

public class PlayerController : MonoBehaviour {

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private float speed = 3;
    private Vector3 move;

    private bool isStopped = false;

    void Start() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    void Update() {

        if (isStopped) return;

        move = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            move += new Vector3(-1, 0, 0);
            //Debug.LogError("왼쪽");
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            move += new Vector3(1, 0, 0);
            //Debug.LogError("오른쪽");
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            move += new Vector3(0, 1, 0);
            //Debug.LogError("위쪽");
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            move += new Vector3(0, -1, 0);
            //Debug.LogError("아래쪽");
        }
        move = move.normalized;
        if (move.x < 0) {
            _spriteRenderer.flipX = true;
        }
        if (move.x > 0) {
            _spriteRenderer.flipX = false;
        }

        if (move.magnitude > 0) {
            _animator.SetTrigger("Move");
        } else {
            _animator.SetTrigger("Stop");
        }
    }

    private void FixedUpdate() {
        transform.Translate(move * speed * Time.deltaTime);
    }

    public void StopMovement() {
        isStopped = true;
    }
}
