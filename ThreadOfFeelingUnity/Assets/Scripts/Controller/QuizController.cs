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
        [Header("���� UI")]
        [Tooltip("���� UI�� �θ� �г�")]
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button answerButton1;
        [SerializeField] private Button answerButton2;

        [Header("�ǵ�� UI")]
        [Tooltip("���� ����/���� �ǵ�� �г�")]
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private Button feedbackContinueButton;

        // ���� ���� ����
        private List<Question> currentQuizzes;
        private int currentQuizIndex = 0;
        private bool isAnswer1OnButton1;
        private bool isWaitingForNext = false;

        // �ܺη� �˸� �̺�Ʈ (��� �� ������ ��)
        private Action onAllQuizzesCompleted;

        public bool IsActive => questionPanel.activeInHierarchy || feedbackPanel.activeInHierarchy;

        private void Start() {
            // ��ư �̺�Ʈ ����
            answerButton1.onClick.AddListener(() => OnAnswerClicked(0));
            answerButton2.onClick.AddListener(() => OnAnswerClicked(1));
            
            // �ʱ�ȭ
            questionPanel.SetActive(false);
            feedbackPanel.SetActive(false);
        }

        // StorySceneUi�� Update ������ ȣ��
        public void HandleInput() {
            // InputManager�� ���� ���� ��� �Է°� Ȯ�� (HandMode��� 10 or 20)
            int motionInput = InputManager.Instance.GetMotionInput();

            // 1. ���� ���� �� (���� ������)
            if (questionPanel.activeInHierarchy) {
                // 1�� ����: Ű���� 1 or �޼�(10)
                if (InputManager.Instance.GetNOneKeyDown() || motionInput == 10) {
                    OnAnswerClicked(0);
                }
                // 2�� ����: Ű���� 2 or ������(20)
                else if (InputManager.Instance.GetNTwoKeyDown() || motionInput == 20) {
                    OnAnswerClicked(1);
            }
            // 2. ��� Ȯ�� �� (���� �Ѿ��)
            else if (feedbackPanel.activeInHierarchy) {
                // �����̽��� �Է� �� ��������
                if (InputManager.Instance.GetSpaceKeyDown()) {
                    SoundManager.Instance.SelectSound();
                    if (isWaitingForNext) ShowNextQuiz();
                    else RetryQuiz();
                }
            }
        }
        
        // �ܺο��� ���� ������ ��û�� �� ȣ��
        public void StartQuizSequence(List<Question> quizzes, Action onComplete) {
            InputManager.Instance.SetHandMode();

            currentQuizzes = quizzes;
            onAllQuizzesCompleted = onComplete;
            currentQuizIndex = 0;
            ShowQuiz();
        }

        // ��� �ν��� �޾ƿ��� �Լ�
        public int GetMotionInput() {
            // EmotionManager�� ������ 0 ����
            if (EmotionManager.Instance == null) return 0;
            return EmotionManager.Instance.GetEmotion();
        }


        private void ShowQuiz() {
            // ���� ��ȿ�� �˻�
            if (currentQuizzes == null || currentQuizIndex >= currentQuizzes.Count) {
                EndQuizSequence();
                return;
            }

            Question q = currentQuizzes[currentQuizIndex];
            
            questionPanel.SetActive(true);
            feedbackPanel.SetActive(false);

            // ���� �ؽ�Ʈ (��ȣ ����)
            questionText.text = $"���� #{currentQuizIndex + 1}. {q.questionText}";

            // ������ ���� ��ġ
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
            // �г��� ���������� �Է� ����
            if (!questionPanel.activeInHierarchy) return;
            SoundManager.Instance.SelectSound();
            Question q = currentQuizzes[currentQuizIndex];
            
            questionPanel.SetActive(false);
            feedbackPanel.SetActive(true);
            
            EventSystem.current.SetSelectedGameObject(null);
            feedbackContinueButton.onClick.RemoveAllListeners();

            int logicalAnswerIndex = isAnswer1OnButton1 ? clickedButtonIndex : 1 - clickedButtonIndex;

            if (logicalAnswerIndex == q.correctAnswerIndex) {
                // [����]
                SoundManager.Instance.RightSound();
                feedbackText.text = q.correctFeedback;
                SetupFeedbackButton("����", ShowNextQuiz);
                isWaitingForNext = true;
                Debug.Log($"[QuizController] ����! ({currentQuizIndex + 1}/{currentQuizzes.Count})");
            }
            else {
                // [����]
                SoundManager.Instance.WrongSound();
                feedbackText.text = q.wrongFeedback;
                SetupFeedbackButton("�ٽ� �õ�", RetryQuiz);
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
            Debug.Log("[QuizController] ��� ���� �Ϸ�");
            onAllQuizzesCompleted?.Invoke(); // StorySceneUi�� �˸�
        }
    }
}