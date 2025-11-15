using System.Collections.Generic;
using Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    // 게임 상태 관리
    public GameState CurrentState { get; private set; }
    private Stack<GameState> stateStack = new Stack<GameState>();

    // 플레이어 관련
    [SerializeField] private GameObject maleCharacterPrefab;    // 남성 캐릭터 프리팹
    [SerializeField] private GameObject femaleCharacterPrefab;  // 여성 캐릭터 프리팹
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0, 0); // 시작 스폰 지점
    private GameObject playerInstance;        // 생성된 플레이어 캐릭터의 인스턴스
    private PlayerController playerController; // 플레이어 컨트롤러

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
            SetState(GameState.Village);
        }
        else {
            playerInstance = null;
            playerController = null;
            if (scene.name == "SelectionScene") {
                SetState(GameState.Selection);
            }
            else if (scene.name == "StoryScene") {
                SetState(GameState.Story);
            }
            else if (scene.name == "HousingScene") {
                SetState(GameState.Housing);
            }
            else {
                Debug.Log("[GameManager] 현재 씬은 " + scene.name + "입니다");
                SetState(GameState.Loading);
            }
        }
    }
    
    // 마을 씬 시작 시 플레이어 생성
    public void StartGame() {
        // 이미 플레이어가 있다면 중복 스폰 방지
        if (playerInstance != null) return; 

        ChildProfile userProfile = DataManager.Instance.currentProfile;

        if (userProfile == null) {
            Debug.LogError("[GameManager] 선택된 프로필이 없습니다");
            return;
        }

        GameObject prefabToCreate = (userProfile.gender == Gender.Male) ? maleCharacterPrefab : femaleCharacterPrefab;

        if (prefabToCreate != null) {
            playerInstance = Instantiate(prefabToCreate, spawnPosition, Quaternion.identity);
            playerController = playerInstance.GetComponent<PlayerController>();
        }
        else {
            Debug.LogError("[GameManager] 성별에 맞는 캐릭터 프리팹이 GameManager에 연결되지 않았습니다");
        }
    }

    // 게임 상태 제어
    public void SetState(GameState newState) {
        if (CurrentState == newState) return;
        CurrentState = newState;
        Debug.Log("[GameManager] 게임 상태 변경: " + CurrentState);

        if (playerController == null) {
            if (playerInstance != null) {
                playerController = playerInstance.GetComponent<PlayerController>();
            }
        }

        if (playerController != null) {
            switch (CurrentState) {
                case GameState.Village:
                    playerController.ResumeMovement();
                    break;
                case GameState.Story:
                case GameState.Housing:
                case GameState.Selection:
                case GameState.Paused:
                case GameState.Loading:
                    playerController.StopMovement();
                    break;
            }
        }
        else {
            if (CurrentState == GameState.Village || CurrentState == GameState.Story || CurrentState == GameState.Housing) {
                Debug.LogWarning("[GameManager] " + CurrentState + " 상태로 변경하려 하나, PlayerController가 없습니다. (씬에 플레이어 없음)");
            }
        }
        
    }

    // 게임 일시정지
    public void PauseGame() {
        if (CurrentState == GameState.Paused) return;
        stateStack.Push(CurrentState);
        SetState(GameState.Paused);
    }

    // 게임 재개
    public void ResumeGame() {
        if (CurrentState != GameState.Paused) return;
        if (stateStack.Count > 0) {
            SetState(stateStack.Pop());
        }
        else {
            SetState(GameState.Village);
            Debug.LogWarning("[GameManager] 스택에 상태가 비어있습니다");
        }
    }

    // 씬 로드 시 등록된 함수를 해제
    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // -- 씬 이동 함수들 --
    public void LoadStoryScene() {
        SceneManager.LoadScene("StoryScene");
        stateStack.Clear();
    }
    public void LoadHousingScene() {
        SceneManager.LoadScene("HousingScene");
        stateStack.Clear();
    }
    public void LoadVillageScene() {
        SceneManager.LoadScene("MainGameScene");
        stateStack.Clear();
    }
    public void LoadSelectionScene() {
        SceneManager.LoadScene("SelectionScene");
        stateStack.Clear();
    }
}
}