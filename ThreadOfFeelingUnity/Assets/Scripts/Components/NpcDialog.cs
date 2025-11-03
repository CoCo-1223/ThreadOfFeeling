using UnityEngine;
using UnityEngine.UI;

public class NpcDialog : MonoBehaviour {
    public Text talkText;
    public GameObject scanObject;
    
    public void Action(GameObject scanObj) {
        scanObject = scanObj;
        talkText.text = "이것은" + scanObj.name + "이다.";
    }
}
