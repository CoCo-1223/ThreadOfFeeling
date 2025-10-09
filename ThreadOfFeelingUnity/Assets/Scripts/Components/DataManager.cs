using UnityEngine;
using UnityEngine.Analytics;

public enum Gender {
    Male,
    Female
}

public class DataManager : MonoBehaviour {
    public static DataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    public class Profile {
        public string Name;
        public int Age;
        public Gender UserGender;
    }

    public Profile currentProfile;

    public void SelectProfile(Profile profile) {
        currentProfile = profile;
    }

    public void SaveProfileData() {
        // currentProfile의 정보를 SQLite에 저장
    }

}