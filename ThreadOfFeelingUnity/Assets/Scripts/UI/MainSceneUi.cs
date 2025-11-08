using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainSceneUI : SceneUI {

    [Header("Talk UI (From MainSceneUI)")]
    [Tooltip("대화창 패널 게임 오브젝트")]
    [SerializeField] private GameObject talkPanel;
    [Tooltip("대화 내용이 표시될 Text")]
    [SerializeField] private TextMeshProUGUI objectText;
    [Tooltip("NPC 초상화가 표시될 Image")]
    [SerializeField] private Image portraitImg;

    [Header("Choice Buttons (From MainSceneUI)")]
    [Tooltip("동화 선택 버튼")]
    [SerializeField] private GameObject choiceBttnStory;
    [Tooltip("하우징 선택 버튼")]
    [SerializeField] private GameObject choiceBttnHousing;

    protected override void Start() {
        base.Start();
        HideTalkPanel();
        ShowChoiceButtons(false);
    }

    protected override void Update() {
        base.Update();
    }

    public void ShowTalkPanel(string text, Sprite portrait, bool isNPC) {
        talkPanel.SetActive(true);
        objectText.text = text;

        if (isNPC && portrait != null) {
            // NPC이고 초상화가 있으면 표시
            portraitImg.sprite = portrait;
            portraitImg.color = new Color(1, 1, 1, 1); // 완전 불투명
        }
        else {
            // NPC가 아니거나 초상화가 없으면 숨김
            portraitImg.sprite = null;
            portraitImg.color = new Color(1, 1, 1, 0); // 완전 투명
        }
    }

    public void HideTalkPanel() {
        talkPanel.SetActive(false);
    }

    public void ShowChoiceButtons(bool show) {
        choiceBttnStory.SetActive(show);
        choiceBttnHousing.SetActive(show);
    }
}
