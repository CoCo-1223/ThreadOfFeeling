using Unity.VisualScripting;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    private Vector3 _moveInput;
    private Vector3 _facingDirection;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        // 이동 입력 계산
        Vector3 rawInput = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) rawInput.x = -1;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) rawInput.x = 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) rawInput.y = 1;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) rawInput.y = -1;
        _moveInput = rawInput.normalized;


        // 바라보는 방향 계산
        if (rawInput.y > 0) _facingDirection = Vector3.up;
        else if (rawInput.y < 0) _facingDirection = Vector3.down;
        else if (rawInput.x < 0) _facingDirection = Vector3.left;
        else if (rawInput.x > 0) _facingDirection = Vector3.right;
    }
    public Vector3 GetMoveInput() {
        return _moveInput;
    }

    public Vector3 GetFacingDirection() {
        return _facingDirection;
    }

    public bool GetInteractionKeyDown() {
        return Input.GetButtonDown("Jump");
    }
}