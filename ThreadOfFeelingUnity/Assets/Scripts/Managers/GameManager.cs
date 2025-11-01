using UnityEngine;
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public GameObject maleCharacterPrefab;     // 남성 캐릭터 프리팹
    public GameObject femaleCharacterPrefab;   // 여성 캐릭터 프리팹
    public Vector3 spawnPosition = new Vector3(0, 0, 0); // 시작 스폰 지점
    private GameObject playerInstance;          // 플레이어 캐릭터
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start() {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGameScene") {
            StartGame();
        }
        else {
            Debug.Log("현재 씬은 " + currentSceneName + "입니다");
        }
    }

    // 시작 마을 진입
    public void StartGame() {
        ChildProfile userProfile = DataManager.Instance.currentProfile;

        if (userProfile == null) {
            Debug.LogError("선택된 프로필이 없습니다");
            return;
        }

        GameObject prefabToCreate = null;

        if (userProfile.gender == Gender.Male) {
            prefabToCreate = maleCharacterPrefab;
        }
        else {
            prefabToCreate = femaleCharacterPrefab;
        }

        if (prefabToCreate != null) {
            playerInstance = Instantiate(prefabToCreate, spawnPosition, Quaternion.identity);
        }
        else {
            Debug.LogError("성별에 맞는 캐릭터 프리팹이 GameManager에 연결되지 않았습니다");
        }
    }
    
    // 게임 멈추기
    public void PauseGame() {
        if (playerInstance != null) {
            playerInstance.GetComponent<PlayerController>().StopMovement();
        }
    }

    // 동화 선택
    // 동화 진행
    // 하우징
}