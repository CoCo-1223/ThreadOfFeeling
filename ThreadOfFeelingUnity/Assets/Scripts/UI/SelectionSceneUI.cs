using Components;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SelectionSceneUi : SceneUI {
        [Header("Story Details Popup")]
        [Tooltip("상세 정보 팝업의 부모 Panel 오브젝트")]
        [SerializeField] private GameObject storyDetailsPanel;
        [Tooltip("팝업에 표시할 동화 제목 Text")]
        [SerializeField] private TextMeshProUGUI detailTitle;
    
        [Tooltip("팝업에 표시할 동화 커버 Image")]
        [SerializeField] private Image detailCoverImage; 

        [Tooltip("팝업에 표시할 동화 설명 Text")]
        [SerializeField] private TextMeshProUGUI detailDescription;
        [Tooltip("팝업에 표시할 동화 태그 Text")]
        [SerializeField] private TextMeshProUGUI detailTag;
        [Tooltip("팝업의 '시작하기' 버튼")]
        [SerializeField] private Button startButton;

        [Header("Story Type Selection")]
        [Tooltip("타입 A, B, C 선택 토글들 (순서대로 0:A, 1:B, 2:C)")]
        [SerializeField] private Toggle[] typeToggles; 
        
        [Tooltip("토글 그룹 (선택 사항, 하나만 선택되게 하려면 필수)")]
        [SerializeField] private ToggleGroup typeToggleGroup;

        private Story pendingStory;
        private StoryType currentSelectedType = StoryType.TypeA;

        protected override void Start(){
            base.Start();
            storyDetailsPanel.SetActive(false);

            // 토글 이벤트 연결
            for (int i = 0; i < typeToggles.Length; i++) {
                int index = i; // 람다식 캡처 문제 방지
                typeToggles[i].onValueChanged.AddListener((isOn) => {
                    if (isOn) {
                        OnTypeChanged((StoryType)index);
                    }
                });
            }
        }

        protected override void Update() {
            base.Update();
        }

        public void ShowStoryDetails(Story data) {
            if (data == null) return;
            SoundManager.Instance.SelectSound();
            pendingStory = data;

            if (detailTitle != null) detailTitle.text = data.storyTitle;

            if (detailCoverImage != null) {
                detailCoverImage.sprite = data.storyCoverImage;
                detailCoverImage.gameObject.SetActive(data.storyCoverImage != null);
            }

            if (detailDescription != null) detailDescription.text = data.storyDescription;
            if (detailTag != null) detailTag.text = data.storyTag;

            // 팝업이 열릴 때 항상 첫 번째(Type A)가 선택된 상태로 초기화
            if (typeToggles.Length > 0) {
                // 이벤트가 발생하여 OnTypeChanged도 자동으로 호출됨
                typeToggles[0].isOn = true; 
            }

            storyDetailsPanel.SetActive(true);
        }

        // 토글 상태가 변경되었을 때 호출됨
        private void OnTypeChanged(StoryType type) {
            currentSelectedType = type;
            Debug.Log($"[SelectionSceneUi] 스토리 타입 변경됨: {currentSelectedType}");
        }

        public void OnClickStartStory() {
            SoundManager.Instance.SelectSound();
            if (pendingStory == null) {
                Debug.LogError("[SelectionSceneUi] 시작할 동화가 선택되지 않았습니다.");
                return;
            }
            DataManager.Instance.SelectFairyTaleData(pendingStory, currentSelectedType);
            storyDetailsPanel.SetActive(false);
            GameManager.Instance.LoadStoryScene();
        }

        public void OnClickClosePopup() {
            SoundManager.Instance.SelectSound();
            storyDetailsPanel.SetActive(false);
            pendingStory = null;
        }
    }
}