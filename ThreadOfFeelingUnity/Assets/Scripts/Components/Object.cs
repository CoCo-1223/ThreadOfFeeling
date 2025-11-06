using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class Object : MonoBehaviour {
    public int id;
    public bool isNPC;
    public GameObject scanObject;
    private bool isAction = false;
    public int talkIndex;
    

    public void Action(GameObject scanObj) {
        scanObject = scanObj;
        Talk(id, isNPC);
        DataManager.Instance.talkPanel.SetActive(isAction);
    }

    public bool getIsAction() {
        return isAction;
    }

    public void Talk(int id, bool isNPC) {
        string talkData = DataManager.Instance.GetTalkData(id, talkIndex);
        if (talkData == null) {
            isAction = false;
            talkIndex = 0;
            if (DataManager.Instance.choiceBttnStory != null) {
                DataManager.Instance.choiceBttnStory.SetActive(false);
                DataManager.Instance.choiceBttnHousing.SetActive(false);
            }
            return;
        }
        string[] parts = talkData.Split(':');
        if (isNPC) {
            DataManager.Instance.ObjectText.text = parts[0];
            DataManager.Instance.portraitImg.sprite = DataManager.Instance.GetPortrait(id, int.Parse(parts[1]));
            DataManager.Instance.portraitImg.color = new Color(1, 1, 1, 1);
            if (parts.Length > 2 && parts[2] == "CHOICE") {
                DataManager.Instance.choiceBttnStory.SetActive(true);
                DataManager.Instance.choiceBttnHousing.SetActive(true);
            }
            else {
                DataManager.Instance.choiceBttnStory.SetActive(false);
                DataManager.Instance.choiceBttnHousing.SetActive(false);
            }
        }
        else {
            DataManager.Instance.ObjectText.text = talkData;
            DataManager.Instance.portraitImg.color = new Color(1, 1, 1, 0);
        }
            
        isAction = true;
        talkIndex++;
    }
}