using UnityEngine;

public class MainSceneUi : MonoBehaviour {
    public void OnClickSave() {
        GameManager.Instance.GameSave();
    }

    public void OnClickExit() {
        GameManager.Instance.GameExit();
    }

    public void OnClickResume() {
        GameManager.Instance.MenuSet(false);
    }

    public void OnClickGoToStory() {
        GameManager.Instance.LoadStoryScene();
    }

    public void OnClickGoToHousing() {
        GameManager.Instance.LoadHousingScene();
    }
}
