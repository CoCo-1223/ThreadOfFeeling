using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    // 게임 상태 관리
    public GameState CurrentState { get; private set; }

    // 플레이어 스폰 관련
    public GameObject maleCharacterPrefab;    // 남성 캐릭터 프리팹
    public GameObject femaleCharacterPrefab;  // 여성 캐릭터 프리팹
    public Vector3 spawnPosition = new Vector3(0, 0, 0); // 시작 스폰 지점
    private GameObject playerInstance;        // 생성된 플레이어 캐릭터의 인스턴스
    private PlayerController playerController; // 플레이어 컨트롤러
    //public NpcDialog dialogueManager;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        // 씬이 로드될 때마다 호출될 함수를 등록
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 로드된 씬에서도 즉시 1회 실행
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // 씬이 로드될 때마다 호출되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainGameScene") {
            StartGame();
            SetState(GameState.Playing); // 게임 시작 시 상태를 'Playing'으로 설정
        }
        else {
            Debug.Log("현재 씬은 " + scene.name + "입니다 (GameManager)");
            // 다른 씬에서는 플레이어를 스폰 안함
            playerInstance = null;
            playerController = null;
        }
    }

    // 메인 게임 씬 시작 시 플레이어 스폰
    public void StartGame() {
        // 이미 플레이어가 있다면 중복 스폰 방지
        if (playerInstance != null) return; 

        ChildProfile userProfile = DataManager.Instance.currentProfile;

        if (userProfile == null) {
            Debug.LogError("선택된 프로필이 없습니다 (GameManager)");
            // 테스트용 기본 프로필
            userProfile = new ChildProfile("TestUser", 0, Gender.Male);
            return;
        }

        GameObject prefabToCreate = (userProfile.gender == Gender.Male) ? maleCharacterPrefab : femaleCharacterPrefab;

        if (prefabToCreate != null) {
            playerInstance = Instantiate(prefabToCreate, spawnPosition, Quaternion.identity);
            playerController = playerInstance.GetComponent<PlayerController>();

            //if (playerController != null && dialogueManager != null) playerController.dialog = dialogueManager;
            //else Debug.LogError("플레이어 스폰 또는 NpcDialog 연결 실패");
            
        }
        else {
            Debug.LogError("성별에 맞는 캐릭터 프리팹이 GameManager에 연결되지 않았습니다");
        }
    }

    // 게임 상태 제어
    public void SetState(GameState newState) {
        if (CurrentState == newState) return; // 이미 같은 상태라면 변경 안 함

        CurrentState = newState;

        if (playerController == null) {
            // MainGameScene이 아닌 경우 playerController가 null
            if (CurrentState != GameState.Playing && SceneManager.GetActiveScene().name != "MainGameScene") {
                // MainGameScene이 아닌데 다른 상태로 변경 시도
            }
            else if (CurrentState == GameState.Playing && playerInstance == null && SceneManager.GetActiveScene().name == "MainGameScene") {
                Debug.LogWarning("플레이어가 아직 스폰되지 않았거나 캐싱에 실패했습니다");
            }
            return;
        }

        // 상태에 따라 플레이어의 행동을 제어
        switch (CurrentState) {
            case GameState.Playing:
                playerController.ResumeMovement();
                break;
            
            case GameState.Dialogue:
                playerController.StopMovement();
                break;
            
            case GameState.Paused:
                playerController.StopMovement();
                break;
            
            case GameState.Loading:
                playerController.StopMovement();
                break;
            
            case GameState.GameOver:
                playerController.StopMovement();
                break;
        }
    }

    
    // NPC 대화 시작
    public void StartDialogue() {
        SetState(GameState.Dialogue);
    }

    // NPC 대화 종료
    public void EndDialogue() {
        SetState(GameState.Playing);
    }

    // 게임 일시정지(UI 버튼)
    public void PauseGame() {
        SetState(GameState.Paused);
    }

    // 게임 재개(UI 버튼)
    public void ResumeGame() {
        SetState(GameState.Playing);
    }

    // 씬 로드 시 등록된 함수를 해제
    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

