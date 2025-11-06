using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


// 게임의 모든 데이터(프로필, 대화, 퀘스트)를 관리하는 매니저

public class DataManager : MonoBehaviour {
    public static DataManager Instance { get; private set; }

    // 현재 선택된 사용자 프로필
    public ChildProfile currentProfile;
    
    // Object 상호작용
    public TextMeshProUGUI ObjectText;
    public GameObject talkPanel;
    Dictionary<int, string[]> talkData;
    Dictionary<int, Sprite> portraitData;
    public Sprite[] portraitArr;
    public Image portraitImg;

    // 씬 전환 버튼
    public GameObject choiceBttnStory;
    public GameObject choiceBttnHousing;

    public TextMeshProUGUI menuUserName;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            //OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            // 테스트용 프로필 생성
            currentProfile = new ChildProfile("TestUser", 0, Gender.Male);
            
            // 게임 데이터 로드
            LoadViallageData();
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Village 씬 로드시 연결할 데이터
        if (scene.name == "MainGameScene") {
            try {
                GameObject canvasObj = GameObject.Find("Canvas");
                if (canvasObj == null) {
                    Debug.LogError("[DataManager] 'Canvas' 오브젝트를 씬에서 찾을 수 없습니다.");
                    return;
                }

                talkPanel = canvasObj.transform.Find("TalkPanel").gameObject;
                ObjectText = canvasObj.transform.Find("TalkPanel/Text").GetComponent<TextMeshProUGUI>();
                portraitImg = canvasObj.transform.Find("TalkPanel/Portrait").GetComponent<Image>();
                choiceBttnStory = canvasObj.transform.Find("TalkPanel/StoryBttn").gameObject;
                choiceBttnHousing = canvasObj.transform.Find("TalkPanel/HousingBttn").gameObject;
                menuUserName = canvasObj.transform.Find("MenuSet/User Name/text").GetComponent<TextMeshProUGUI>();

                if (talkPanel != null)
                    talkPanel.SetActive(false);
                if (choiceBttnStory != null)
                    choiceBttnStory.SetActive(false);
                if (choiceBttnHousing != null)
                    choiceBttnHousing.SetActive(false);
                if (menuUserName != null && currentProfile != null)
                    menuUserName.text = currentProfile.nickname;
            }
            catch (System.Exception e) {
                Debug.LogError("[DataManager] UI 컴포넌트를 찾는 중 예외 발생. 씬의 UI 오브젝트 이름과 Find()의 경로가 일치하는지 확인하세요: " + e.Message);
            }
        }
        else {
            talkPanel = null;
            ObjectText = null;
            portraitImg = null;
            choiceBttnStory = null;
            choiceBttnHousing = null;
            menuUserName = null;
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LoadViallageData() {
        talkData = new Dictionary<int, string[]>();
        portraitData = new Dictionary<int, Sprite>();
        GenerateObjectData();
    }

    public void GenerateObjectData() {
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

    // 프로필 저장/로드 함수 추가
    public void SaveProfileData() { }
    public void LoadProfileData() { }
}