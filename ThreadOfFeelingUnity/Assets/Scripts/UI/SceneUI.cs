using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;

namespace UI
{
    public class SceneUI : MonoBehaviour {

        [Header("Menu UI (From SceneUI)")]
        [SerializeField] private GameObject menuSet;

        [Header("Profile Info UI")]
        [SerializeField] private TextMeshProUGUI menuUserName;
        [SerializeField] private TextMeshProUGUI profileAgeText;
        [SerializeField] private TextMeshProUGUI profileGenderText;

        [Tooltip("7각형 능력치 그래프")]
        [SerializeField] private RadarChart statChart;
        
        [Header("Settings UI")]
        [Tooltip("TTS 사용 여부를 설정할 토글 버튼")]
        [SerializeField] private Toggle ttsToggle;

        protected virtual void Start() {
            UpdateProfileUI();
            if (menuSet != null) {
                menuSet.SetActive(false);
            }
            else {
                Debug.LogError("[SceneUI] 'MenuSet' 게임 오브젝트가 인스펙터에 연결되지 않았습니다");
            }
            // 파일 브라우저 초기 설정 (가로/세로 모드 등)
            FileBrowser.SetFilters(true, new FileBrowser.Filter("JSON Files", ".json"));
            FileBrowser.SetDefaultFilter(".json");
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

            if (ttsToggle != null) {
                ttsToggle.onValueChanged.RemoveAllListeners(); // 중복 방지
                ttsToggle.onValueChanged.AddListener(OnTtsToggleChanged);
            }
        }

        protected virtual void Update() {
            if (InputManager.Instance.GetEscapeKeyDown()) {
                if (menuSet != null) {
                    MenuSet(!menuSet.activeSelf);
                }
            }
        }

        public void OnClickLoadExternalData() {
            SoundManager.Instance.SelectSound();

            // 코루틴으로 파일 브라우저 열기 실행
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        IEnumerator ShowLoadDialogCoroutine() {
            // 1. 파일 열기 다이얼로그 표시
            // 매개변수: (성공콜백, 취소콜백, 선택모드, 다중선택여부, 초기경로, 초기파일, 제목, 버튼이름)
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "체크리스트 불러오기", "선택");

            // 2. 결과 확인
            if (FileBrowser.Success) {
                // 선택된 파일 경로 가져오기 (배열 형태)
                string path = FileBrowser.Result[0];
                Debug.Log($"선택된 파일 경로: {path}");

                // 3. DataManager를 통해 데이터 로드 및 적용
                DataManager.Instance.LoadChecklistFile(path);

                // 4. UI 갱신 (그래프 등 즉시 반영)
                UpdateProfileUI();
            }
            else {
                // 취소했을 때
                Debug.Log("파일 선택이 취소되었습니다.");
            }
        }

        private void UpdateProfileUI() {
            if (DataManager.Instance == null || DataManager.Instance.currentProfile == null) {
                return;
            }

            var profile = DataManager.Instance.currentProfile;

            // 닉네임
            if (menuUserName != null) menuUserName.text = profile.Nickname;
            
            // 나이
            if (profileAgeText != null) 
                profileAgeText.text = profile.AgeBandText; 
            
            // 성별
            if (profileGenderText != null) 
                profileGenderText.text = profile.GenderText;

            // TTS 설정 상태
            if (ttsToggle != null) ttsToggle.SetIsOnWithoutNotify(profile.IsTtsUsed);

            if (statChart != null && profile.Stats != null) {
                // 최대 능력치 값을 100으로 가정하고 정규화해서 전달
                statChart.SetStats(profile.Stats.GetNormalizedValues(100f));
            }
        }

        // TTS 토글 값이 변경될 때 호출됨
        public void OnTtsToggleChanged(bool isOn) {
            if (DataManager.Instance != null && DataManager.Instance.currentProfile != null) {
                SoundManager.Instance.SelectSound();
                
                // 1. 데이터 저장
                DataManager.Instance.currentProfile.IsTtsUsed = isOn;
                Debug.Log($"[SceneUI] TTS 설정 변경됨: {isOn}");
                
                // 2. 사운드 매니저에 알림 (꺼졌으면 즉시 멈춤)
                SoundManager.Instance.OnTtsToggleChanged(isOn);
            }
        }

        // 메뉴창 활성화 or 비활성화
        private void MenuSet(bool isActive) {
            if (menuSet == null) return;
            menuSet.SetActive(isActive);
            if (isActive) {
                GameManager.Instance.PauseGame();
                SoundManager.Instance.SelectSound(true);
                // 메뉴 열 때 프로필 정보 최신화
                UpdateProfileUI();
            }
            else {
                GameManager.Instance.ResumeGame();
                SoundManager.Instance.SelectSound(false);
            }
        }

        // 게임 저장 - 메뉴
        public void OnClickSave() {
            DataManager.Instance.SaveProfileData();
            Debug.Log("[SceneUI] 게임 저장 완료");
            MenuSet(false);
        }

        // 게임 끝내기 - build시에만 작동
        public void OnClickExit() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            Application.Quit();
        }

        // 이어하기 - 메뉴
        public void OnClickResume() {
            MenuSet(false);
        }

        // 씬 이동 함수 (버튼과 연결)
        public void OnClickGoToStory() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadStoryScene();
        }
        public void OnClickGoToHousing() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadHousingScene();
        }
        public void OnClickGoToVillage() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadVillageScene();
        }
        public void OnClickGoToSelection() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadSelectionScene();
        }
        public void OnClickGoToProfile() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadProfileScene();
        }
        public void OnClickGoToStart() {
            SoundManager.Instance.SelectSound();
            SoundManager.Instance.StopTTS();
            GameManager.Instance.LoadStartScene();
        }
    }
}
