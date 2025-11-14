using Unity.VisualScripting;
using UnityEngine;
using EmotionManager;
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
        // ÀÌµ¿ ÀÔ·Â °è»ê
        Vector3 rawInput = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) rawInput.x = -1;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) rawInput.x = 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) rawInput.y = 1;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) rawInput.y = -1;
        _moveInput = rawInput.normalized;

    }
    public Vector3 GetMoveInput() {
        return _moveInput;
    }

    public bool GetSpaceKeyDown() {
        return Input.GetButtonDown("Jump");
    }

    public bool GetNOneKeyDown() {
        return Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1);
    }

    public bool GetNTwoKeyDown() {
        return Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2);
    }
    
    public bool GetEscapeKeyDown() {
        return Input.GetButtonDown("Cancel");
    }

    public int GetMotionInput() {
        return EmotionManager.Instance.GetEmotion();
    }
}
