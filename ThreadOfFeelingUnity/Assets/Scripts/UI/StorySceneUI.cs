using Components;
using Controller;
using Managers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StorySceneUi : SceneUI {
        [Header("스토리 UI")]
        [Tooltip("동화 이미지를 표시할 Image UI")]
        [SerializeField] private Image storyDisplayImage;
        
        [Tooltip("대사 텍스트가 포함된 부모 패널")]
        [SerializeField] private GameObject dialoguePanel; 
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("퀴즈/보상 컨트롤러 연결")]
        [SerializeField] private QuizController quizController;
        [Tooltip("보상 팝업 로직을 담당하는 스크립트")]
        [SerializeField] private RewardUI rewardPopup;

        private Story currentTale;
        private StoryType currentType; 
        private int currentScenarioIndex = 0;
        private Scenario currentScenario; 
     
        // 퀴즈 중인지 여부는 컨트롤러를 통해 확인하거나 로직으로 관리
        private bool IsQuizMode => quizController != null && quizController.IsActive;

        protected override void Start() {
            base.Start();

            // 컨트롤러 자동 찾기
            if (quizController == null) quizController = GetComponentInChildren<QuizController>();
            if (rewardPopup == null) rewardPopup = GetComponentInChildren<RewardUI>();

            // 보상 팝업 초기화
            if (rewardPopup != null) rewardPopup.Init();

            currentTale = DataManager.Instance.selectedTale;

            if (currentTale == null || currentTale.scenarios.Count == 0) {
                Debug.LogError("[StorySceneUi] 선택된 동화 데이터가 없습니다.");
                GameManager.Instance.LoadSelectionScene();
                return;
            }
            currentScenarioIndex = 0;
            ShowCurrentScenario();
        }

        protected override void Update() {
            base.Update();

            // 1. 퀴즈 모드일 때: 입력을 퀴즈 컨트롤러에 위임
            if (IsQuizMode) {
                quizController.HandleInput();
                return;
            }

            // 2. 스토리 모드일 때: 스페이스바로 진행
            if (InputManager.Instance.GetSpaceKeyDown()) {
                SoundManager.Instance.SelectSound();
                CheckAndStartQuizOrNext();
            }
        }

        private void CheckAndStartQuizOrNext() {
            List<Question> validQuestions = new List<Question>();

            // 시나리오는 그대로 두고, "퀴즈"만 현재 타입에 맞는 걸 뽑아냅니다.
            if (currentScenario.quizzes != null)
            {
                validQuestions = currentScenario.quizzes
                    .Where(q => q.isCommon || q.targetType == currentType)
                    .ToList();
            }
            
            if (validQuestions.Count > 0) {
                // 필터링된 퀴즈가 있다면 퀴즈 모드 시작
                StartQuizMode(validQuestions);
            }
            else {
                // 퀴즈가 없거나, 내 타입에 맞는 퀴즈가 없다면 바로 다음 장면으로
                ShowNextScenario();
            }
        }

        private void StartQuizMode(List<Question> questions) {
            quizController.StartQuizSequence(questions, OnQuizSequenceFinished);
        }

        // 퀴즈 컨트롤러가 모든 퀴즈를 끝내면 호출됨
        private void OnQuizSequenceFinished() {
            ShowNextScenario();
        }

        public void ShowCurrentScenario() {
            // 스토리 UI 켜기
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            else if (dialogueText != null) dialogueText.gameObject.SetActive(true);

            currentScenario = currentTale.scenarios[currentScenarioIndex];

            if (storyDisplayImage != null)
                storyDisplayImage.sprite = currentScenario.image;
            
            if (dialogueText != null)
                dialogueText.text = currentScenario.dialogueText;

            if (DataManager.Instance.IsTtsUsed()) {
                // TODO: TTS 재생 로직 실행
            }
        }

        public void ShowNextScenario() {
            currentScenarioIndex++;
            if (currentScenarioIndex < currentTale.scenarios.Count) {
                ShowCurrentScenario();
            }
            else {
                HandleStoryEnd();
            }
        }

        private void HandleStoryEnd() {
            // 보상 아이템 지급 (자동 저장)
            if (currentTale.storyReward != null) {
                DataManager.Instance.AddRewardItem(currentTale.storyReward);
            }
    
            // 클리어 기록 저장 (자동 저장)
            DataManager.Instance.AddClearedStory(currentTale.storyId, currentType);

            // UI 표시 위임
            if (currentTale.storyReward != null && rewardPopup != null) {
                // 대화창 숨기기
                if (dialoguePanel != null) dialoguePanel.SetActive(false);
                
                // 팝업 표시 (닫히면 선택 씬으로 이동)
                rewardPopup.Show(currentTale.storyReward, OnClickGoToSelection);
            }
            else {
                // 보상이 없거나 팝업 스크립트가 없으면 바로 이동
                OnClickGoToSelection();
            }
        }
    }
}