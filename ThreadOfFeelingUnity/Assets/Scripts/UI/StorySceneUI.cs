using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StorySceneUi : SceneUI {
    [Header("스토리 UI")]
    [Tooltip("동화 이미지를 표시할 Image UI")]
    [SerializeField] private Image storyDisplayImage;
    [Tooltip("시나리오의 텍스트(대사)를 표시할 Text UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("퀴즈 UI")]
    [Tooltip("퀴즈 UI의 부모 패널 (전체 켜고 끄기 용도)")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button answerButton1;
    [SerializeField] private Button answerButton2;

    [Header("피드백 UI")]
    [Tooltip("퀴즈 정답/오답 피드백 패널")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button feedbackContinueButton;

    private Story currentTale;
    private int currentScenarioIndex = 0;
    private Scenario currentScenario; 
    
    private bool isQuizActive = false;
    private bool isShowingStory = true;

    private bool isAnswer1OnButton1; 

    protected override void Start() {
        base.Start();

        currentTale = DataManager.Instance.selectedTale;

        if (currentTale == null || currentTale.scenarios.Count == 0) {
            Debug.LogError("[StorySceneUi] 선택된 동화 데이터(StoryData)가 없거나 시나리오가 비어있습니다. 선택 씬으로 돌아갑니다.");
            GameManager.Instance.LoadSelectionScene();
            return;
        }

        answerButton1.onClick.AddListener(() => OnAnswerClicked(0));
        answerButton2.onClick.AddListener(() => OnAnswerClicked(1));
        
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(false);

        ShowCurrentScenario();
    }

    protected override void Update() {
        base.Update();

        if (isQuizActive) {
            // 퀴즈 질문이 활성화된 경우 -> 1, 2번 키로 답변
            if (questionPanel.activeInHierarchy) {
                if (InputManager.Instance.GetNOneKeyDown()) {
                    OnAnswerClicked(0);
                }
                else if (InputManager.Instance.GetNTwoKeyDown()) {
                    OnAnswerClicked(1);
                }
            }
            // 오답 피드백 패널이 활성화된 경우 -> 스페이스바로 다시 시도
            else if (feedbackPanel.activeInHierarchy) {
                // 오답 시 다시 시도 버튼을 스페이스바로 누름
                if (InputManager.Instance.GetSpaceKeyDown()) {
                    RetryQuiz();
                }
            }
            return;
        }

        if (InputManager.Instance.GetSpaceKeyDown()) {
            if (isShowingStory) {
                // 스토리 진행 중 스페이스바 -> 퀴즈 또는 다음 시나리오
                bool hasQuiz = currentScenario.quiz != null && !string.IsNullOrEmpty(currentScenario.quiz.questionText);
                if (hasQuiz) {
                    ShowQuiz();
                }
                else {
                    ShowNextScenario();
                }
            }
            else {
                // 정답 피드백 확인 중 스페이스바 -> 다음 시나리오
                // (정답 시 OnAnswerClicked에서 isQuizActive = false, isShowingStory = false가 됨)
                ShowNextScenario();
            }
        }
    }

    public void ShowCurrentScenario() {
        isQuizActive = false;
        isShowingStory = true;
        
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(false);
        
        currentScenario = currentTale.scenarios[currentScenarioIndex];

        if (storyDisplayImage != null)
            storyDisplayImage.sprite = currentScenario.image;
        
        if (dialogueText != null)
            dialogueText.text = currentScenario.dialogueText;
    }

    public void ShowQuiz() {
        isQuizActive = true;
        isShowingStory = false;

        // 퀴즈 UI 설정 및 표시
        questionText.text = currentScenario.quiz.questionText;

        // 퀴즈 선택지 랜덤 배치
        if (Random.value < 0.5f) {
            answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentScenario.quiz.answer1;
            answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentScenario.quiz.answer2;
            isAnswer1OnButton1 = true;
        }
        else {
            answerButton1.GetComponentInChildren<TextMeshProUGUI>().text = currentScenario.quiz.answer2;
            answerButton2.GetComponentInChildren<TextMeshProUGUI>().text = currentScenario.quiz.answer1;
            isAnswer1OnButton1 = false;
        }

        questionPanel.SetActive(true);
    }

    public void OnAnswerClicked(int clickedButtonIndex) {
        questionPanel.SetActive(false);
        feedbackPanel.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(null);


        feedbackContinueButton.onClick.RemoveAllListeners();

        int logicalAnswerIndex;
        if (isAnswer1OnButton1) {
            // 정방향 배치
            logicalAnswerIndex = clickedButtonIndex;
        }
        else {
            // 역방향 배치
            logicalAnswerIndex = 1 - clickedButtonIndex;
        }

        // 정답/오답 확인
        if (logicalAnswerIndex == currentScenario.quiz.correctAnswerIndex) {
            // 정답
            feedbackText.text = currentScenario.quiz.correctFeedback;
            feedbackContinueButton.gameObject.SetActive(false); 
            isQuizActive = false; 
            isShowingStory = false;
        }
        else {
            // 오답
            feedbackText.text = currentScenario.quiz.wrongFeedback;
            feedbackContinueButton.gameObject.SetActive(true);
            feedbackContinueButton.onClick.AddListener(RetryQuiz);
            feedbackContinueButton.GetComponentInChildren<TextMeshProUGUI>().text = "다시 시도";
            isQuizActive = true; 
            isShowingStory = false;
        }
    }

    public void RetryQuiz() {
        feedbackPanel.SetActive(false); 
        questionPanel.SetActive(true); 
        isQuizActive = true;
        isShowingStory = false; 
    }

    public void ShowNextScenario() {
        currentScenarioIndex++;
        if (currentScenarioIndex < currentTale.scenarios.Count) {
            ShowCurrentScenario();
        }
        else {
            Debug.Log($"[StorySceneUi] '{currentTale.storyTitle}' 이야기 끝");
            GiveReward();
            OnClickGoToSelection();
        }
    }

    public void GiveReward() {
        if (currentTale.storyReward != null) {
            Debug.Log($"[StorySceneUi] 보상 획득: {currentTale.storyReward.itemName}");
            // 인벤토리 시스템에 아이템 추가 구현하기
            // DataManager.Instance.Inventory.AddItem(currentTale.storyReward);
        }
    }
}