using UnityEngine;
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public GameObject maleCharacterPrefab; // 남성 캐릭터 프리팹
    public GameObject femaleCharacterPrefab; // 여성 캐릭터 프리팹
    public Transform spawnPoint;

    private GameObject playerInstance; // 생성된 플레이어를 저장할 변수

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void StartGame() {
        DataManager.Profile userProfile = DataManager.Instance.currentProfile;

        if (userProfile == null) {
            Debug.LogError("선택된 프로필이 없습니다");
            return;
        }

        GameObject prefabToCreate = null;

        if (userProfile.UserGender == Gender.Male) {
            prefabToCreate = maleCharacterPrefab;
        }
        else {
            prefabToCreate = femaleCharacterPrefab;
        }

        if (prefabToCreate != null) {
            playerInstance = Instantiate(prefabToCreate, spawnPoint.position, Quaternion.identity);
        }
        else {
            Debug.LogError("성별에 맞는 캐릭터 프리팹이 GameManager에 연결되지 않았습니다!");
        }
    }

    public void PauseGame() {
        if (playerInstance != null) {
            playerInstance.GetComponent<PlayerController>().StopMovement();
        }
    }
}