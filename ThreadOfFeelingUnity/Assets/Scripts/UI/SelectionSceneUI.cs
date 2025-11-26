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
            if(storyDetailsPanel != null)
                storyDetailsPanel.SetActive(false);

            // 토글 이벤트 연결
            if (typeToggles != null) {
                for (int i = 0; i < typeToggles.Length; i++) {
                    int index = i; 
                    if(typeToggles[i] != null) {
                        typeToggles[i].onValueChanged.AddListener((isOn) => {
                            if (isOn) {
                                OnTypeChanged((StoryType)index);
                            }
                        });
                    }
                }
            }
        }

        public void ShowStoryDetails(Story data) {
            if (data == null) {
                Debug.LogError("[SelectionSceneUi] 전달받은 Story 데이터가 null입니다.");
                return;
            }
            
            SoundManager.Instance.SelectSound();
            pendingStory = data;

            if (detailTitle != null) detailTitle.text = data.storyTitle;

            // [수정] 이미지 표시 로직 강화 및 디버그 로그 추가
            if (detailCoverImage != null) {
                if (data.storyCoverImage != null) {
                    detailCoverImage.sprite = data.storyCoverImage;
                    detailCoverImage.color = Color.white; // 혹시 투명해졌을 경우를 대비해 색상 초기화
                    detailCoverImage.gameObject.SetActive(true);
                    // 이미지가 잘 설정되었다면 이 로그가 뜹니다.
                    //Debug.Log($"[SelectionSceneUi] 커버 이미지 설정 완료: {data.storyCoverImage.name}");
                }
                else {
                    // 데이터(ScriptableObject)에 이미지가 비어있으면 이 경고가 뜹니다.
                    Debug.LogWarning($"[SelectionSceneUi] '{data.storyTitle}' 데이터에 커버 이미지가 없습니다 (storyCoverImage is null). Asset을 확인하세요.");
                    detailCoverImage.gameObject.SetActive(false); 
                }
            }
            else {
                Debug.LogError("[SelectionSceneUi] detailCoverImage가 Inspector에 연결되지 않았습니다.");
            }

            if (detailDescription != null) detailDescription.text = data.storyDescription;
            if (detailTag != null) detailTag.text = data.storyTag;

            if (typeToggles != null && typeToggles.Length > 0 && typeToggles[0] != null) {
                typeToggles[0].isOn = true; 
            }

            if(storyDetailsPanel != null)
                storyDetailsPanel.SetActive(true);
        }

        private void OnTypeChanged(StoryType type) {
            currentSelectedType = type;
        }

        public void OnClickStartStory() {
            SoundManager.Instance.SelectSound();
            if (pendingStory == null) {
                Debug.LogError("[SelectionSceneUi] 시작할 동화가 선택되지 않았습니다.");
                return;
            }
            DataManager.Instance.SelectFairyTaleData(pendingStory, currentSelectedType);
            
            if(storyDetailsPanel != null)
                storyDetailsPanel.SetActive(false);
                
            GameManager.Instance.LoadStoryScene();
        }

        public void OnClickClosePopup() {
            SoundManager.Instance.SelectSound();
            if(storyDetailsPanel != null)
                storyDetailsPanel.SetActive(false);
            pendingStory = null;
        }
    }
}