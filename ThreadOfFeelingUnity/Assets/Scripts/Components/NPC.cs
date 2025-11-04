using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPC : MonoBehaviour {
    //public TextMeshProUGUI nameText;
    //public TextMeshProUGUI talkText;
    public GameObject scanObject;
    private bool isAction = false;

    public void Action(GameObject scanObj) {
        if (isAction) {
            isAction = false;
        }
        else {
            isAction = true;
            scanObject = scanObj;
            DataManager.Instance.NpcText.text = "¿Ã∞Õ¿∫ " + scanObj.name;
        }
        DataManager.Instance.talkPanel.SetActive(isAction);
    }

    public bool getIsAction() {
        return isAction;
    }
}