using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// 게임의 모든 데이터(프로필, 대화, 퀘스트)를 관리하는 싱글턴 매니저

public class DataManager : MonoBehaviour
{
     public static DataManager Instance { get; private set; }

    // 현재 선택된 사용자 프로필
    public ChildProfile currentProfile;
    
    // NPC 대화
    public TextMeshProUGUI NpcText;
    public GameObject talkPanel;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- 테스트용 프로필 생성 ---
            currentProfile = new ChildProfile("TestUser", 0, Gender.Male);
            talkPanel.SetActive(false);
            // --- 게임 데이터 로드 ---
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadGameData()
    {

    }

    // (추후 프로필 저장/로드 함수 추가)
    public void SaveProfileData() { }
    public void LoadProfileData() { }
}