using System.Collections.Generic;
using Components;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Managers
{
    public class DataManager : MonoBehaviour {
        public static DataManager Instance { get; private set; }

        // 프로필 리스트 - 프로필 선택화면에서 사용
        public List<ChildProfile> ChildProfiles = new List<ChildProfile>();

        // 현재 선택된 사용자 프로필
        public ChildProfile currentProfile { get; private set; }
    
        // 선택한 동화
        public  Story selectedTale { get; private set; }

        Dictionary<int, string[]> talkData;
        Dictionary<int, Sprite> portraitData;
        public Sprite[] portraitArr;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;

                // 테스트용 프로필 생성
                ChildProfile profile = new ChildProfile("TestUser", 0, Gender.Male);
                SelectProfile(profile);
            
                // 게임 데이터 로드
                LoadViallageData();
            }
            else {
                Destroy(gameObject);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            Debug.Log($"[DataManager] Scene Loaded: {scene.name}");
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // 스토리 데이터 선택
        public void SelectFairyTaleData(Story taleData) {
            selectedTale = taleData;
        }

        // 마을 데이터 생성
        private void LoadViallageData() {
            talkData = new Dictionary<int, string[]>();
            portraitData = new Dictionary<int, Sprite>();
            GenerateObjectData();
        }

        // NPC 데이터들
        private void GenerateObjectData() {
            // NPC별 대화 데이터 "Text:표정"
            talkData.Add(1000, new string[] { 
                "안녕? 난 다비드야:3", 
                "우리 뭐하고 놀까?:0:CHOICE",
            });
            //talkData.Add(2000, new string[] { "안녕?:1", "난 OOO야:2"});
            //talkData.Add(100, new string[] { "평범한 꽃이다"});
            //talkData.Add(200, new string[] { "..."});
            //talkData.Add(300, new string[] { "정체 모를 무언가..."});

            // 초상화 데이터  0: 기본 얼굴, 1: 웃는 얼굴, 2: 화난 얼굴, 3: 놀란 얼굴, 4: 슬픈 얼굴
            portraitData.Add(1000 + 0, portraitArr[0]);
            portraitData.Add(1000 + 1, portraitArr[1]);
            portraitData.Add(1000 + 2, portraitArr[2]);
            portraitData.Add(1000 + 3, portraitArr[3]);
            portraitData.Add(1000 + 4, portraitArr[4]);
            //portraitData.Add(2000 + 0, portraitArr[0]);
            //portraitData.Add(2000 + 1, portraitArr[1]);
            //portraitData.Add(2000 + 2, portraitArr[2]);
            //portraitData.Add(2000 + 3, portraitArr[3]);
        }

        public string GetTalkData(int id, int talkIndex) {
            if (talkIndex == talkData[id].Length)
                return null;
            else
                return talkData[id][talkIndex];
        }

        public Sprite GetPortrait(int id, int portraitIndex) {
            return portraitData[id +  portraitIndex];
        }

        // 현재 프로필에 아이템 추가
        public ChildProfile AddRewardItem(Item item, int amount=1) {
            currentProfile.Inventory.AddItem(item, amount);
            return currentProfile;
        }

        // 현재 프로필에서 아이템 사용
        public ChildProfile UseRewardItem(int itemId, int amount=1) {
            currentProfile.Inventory.UseItem(itemId, amount);
            return currentProfile;
        }

        // 프로필 리스트에서 접속할 프로필 선택
        public ChildProfile SelectProfile(int profileId) {
            currentProfile = ChildProfiles[profileId];
            Debug.Log($"[DataManager] 프로필 선택됨: {currentProfile.Nickname}");
            return currentProfile;
        }

        // Test 유저 생성용
        private void SelectProfile(ChildProfile profile) {
            currentProfile = profile;
            Debug.Log($"[DataManager] 프로필 선택됨: {profile.Nickname}");
        }

        // 프로필 저장/로드 함수 추가
        public void SaveProfileData() { }
        void LoadProfileData() { }
    }
}