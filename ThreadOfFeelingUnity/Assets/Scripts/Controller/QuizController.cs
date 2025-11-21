using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Components;
using PythonManagers;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Controller {
    public class QuizController : MonoBehaviour {
        [Header("퀴즈 UI")]
        [Tooltip("퀴즈 UI의 부모 패널")]
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button answerButton1;
        [SerializeField] private Button answerButton2;

        [Header("피드백 UI")]
        [Tooltip("퀴즈 정답/오답 피드백 패널")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button feedbackContinueButton;

        // 내부 상태 변수
        private List<Question> currentQuizzes;
        private int currentQuizIndex = 0;
        private bool isAnswer1OnButton1;
        private bool isWaitingForNext = false;

        // 외부로 알릴 이벤트 (퀴즈가 다 끝났을 때)
        private Action onAllQuizzesCompleted;

        public bool IsActive => questionPanel.activeInHierarchy || feedbackPanel.activeInHierarchy;

        private void Start() {
            // 버튼 이벤트 연결
            answerButton1.onClick.AddListener(() => OnAnswerClicked(0));
            answerButton2.onClick.AddListener(() => OnAnswerClicked(1));
            
            // 초기화
            questionPanel.SetActive(false);
            feedbackPanel.SetActive(false);
        }

        // StorySceneUi의 Update 문에서 호출
        public void HandleInput() {
            // InputManager를 통해 현재 모션 입력값 확인 (HandMode라면 10 or 20)
            int motionInput = InputManager.Instance.GetMotionInput();

            // 1. 퀴즈 진행 중 (정답 고르기)
            if (questionPanel.activeInHierarchy) {
                // 1번 선택: 키보드 1 or 왼손(10)
                if (InputManager.Instance.GetNOneKeyDown() || motionInput == 10) {
                    OnAnswerClicked(0);
                }
                // 2번 선택: 키보드 2 or 오른손(20)
                else if (InputManager.Instance.GetNTwoKeyDown() || motionInput == 20) {
                    OnAnswerClicked(1);
                }
            }
            // 2. 결과 확인 중 (다음 넘어가기)
            else if (feedbackPanel.activeInHierarchy) {
                // 스페이스바 입력 시 다음으로
                if (InputManager.Instance.GetSpaceKeyDown()) {
                    SoundManager.Instance.SelectSound();
                    if (isWaitingForNext) ShowNextQuiz();
                    else RetryQuiz();
                }
            }
        }
        
        // 외부에서 퀴즈 시작을 요청할 때 호출
        public void StartQuizSequence(List<Question> quizzes, Action onComplete) {
            InputManager.Instance.SetHandMode();

            currentQuizzes = quizzes;
            onAllQuizzesCompleted = onComplete;
            currentQuizIndex = 0;
            ShowQuiz();
        }

        // 모션 인식을 받아오는 함수
        public int GetMotionInput() {
            // EmotionManager가 없으면 0 리턴
            if (EmotionManager.Instance == null) return 0;
            return EmotionManager.Instance.GetEmotion();
        }


        private void ShowQuiz() {
            // 퀴즈 유효성 검사
            if (currentQuizzes == null || currentQuizIndex >= currentQuizzes.Count) {
                EndQuizSequence();
                return;
            }

            Question q = currentQuizzes[currentQuizIndex];
            
            questionPanel.SetActive(true);
            feedbackPanel.SetActive(false);

            // 퀴즈 텍스트 (번호 포함)
            questionText.text = $"퀴즈 #{currentQuizIndex + 1}. {q.questionText}";

            // 선택지 랜덤 배치
            if (Random.value < 0.5f) {
                SetButtonText(answerButton1, q.answer1);
                SetButtonText(answerButton2, q.answer2);
                isAnswer1OnButton1 = true;
            }
            else {
                SetButtonText(answerButton1, q.answer2);
                SetButtonText(answerButton2, q.answer1);
                isAnswer1OnButton1 = false;
            }
        }

        private void SetButtonText(Button btn, string text) {
            btn.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }

        private void OnAnswerClicked(int clickedButtonIndex) {
            // 패널이 꺼져있으면 입력 무시
            if (!questionPanel.activeInHierarchy) return;
            SoundManager.Instance.SelectSound();
            Question q = currentQuizzes[currentQuizIndex];
            
            questionPanel.SetActive(false);
            feedbackPanel.SetActive(true);
            
            EventSystem.current.SetSelectedGameObject(null);
            feedbackContinueButton.onClick.RemoveAllListeners();

            int logicalAnswerIndex = isAnswer1OnButton1 ? clickedButtonIndex : 1 - clickedButtonIndex;

            if (logicalAnswerIndex == q.correctAnswerIndex) {
                // [정답]
                SoundManager.Instance.RightSound();
                feedbackText.text = q.correctFeedback;
                SetupFeedbackButton("다음", ShowNextQuiz);
                isWaitingForNext = true;
                Debug.Log($"[QuizController] 정답! ({currentQuizIndex + 1}/{currentQuizzes.Count})");
            }
            else {
                // [오답]
                SoundManager.Instance.WrongSound();
                feedbackText.text = q.wrongFeedback;
                SetupFeedbackButton("다시 시도", RetryQuiz);
                isWaitingForNext = false;
            }
        }

        private void SetupFeedbackButton(string text, UnityEngine.Events.UnityAction action) {
            feedbackContinueButton.gameObject.SetActive(true);
            feedbackContinueButton.GetComponentInChildren<TextMeshProUGUI>().text = text;
            feedbackContinueButton.onClick.AddListener(action);
        }

        private void RetryQuiz() {
            feedbackPanel.SetActive(false);
            questionPanel.SetActive(true);
        }

        private void ShowNextQuiz() {
            currentQuizIndex++;
            if (currentQuizIndex < currentQuizzes.Count) ShowQuiz();
            else EndQuizSequence();
        }

        private void EndQuizSequence() {
            questionPanel.SetActive(false);
            feedbackPanel.SetActive(false);
            Debug.Log("[QuizController] 모든 퀴즈 완료");
            onAllQuizzesCompleted?.Invoke(); // StorySceneUi에 알림
        }
    }
}