using UnityEngine;

public class StorySceneUi : MonoBehaviour
{
    public void GoToVillage() {
        GameManager.Instance.LoadVillageScene();
    }
    
    void Update()
    {
        
    }
}
