using UnityEngine;
using UnityEngine.Analytics;

public class DataManager : MonoBehaviour {
    public static DataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 테스트 프로필
            ChildProfile testProfile = new ChildProfile("TestUser", 0, Gender.Male);
            //testProfile.nickName = "TestUser";
            //testProfile.ageBand = 0;
            //testProfile.gender = Gender.Male;
            currentProfile = testProfile;

        }
        else Destroy(gameObject);
    }
    

    public ChildProfile currentProfile;

    public void SelectProfile(ChildProfile profile) {
        currentProfile = profile;
    }

    public void SaveProfileData() {
        // currentProfile의 정보를 SQLite에 저장
    }

}
