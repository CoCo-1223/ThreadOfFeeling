unsing UnityEngine;

namespace Managers{
  public enum MotionInputType {
    Emotion,
    Hand
  }

  public class MotionInputManager : MonoBehaviour{
    public static MotionInputManager Instance {get; private set; }

    public MotionInputType inputMode = MotionInputType.EMotion;

    private void Awake() {
      if (Instance != null && Instance != this) {
        Destory(gameObject);
        return;
      }

      Instance =  this;
      DontDestoryOnLoad(gameObject);

    }

    public int GetMotionInput() {
      switch (inputMode) {
        case MotionInputType.Emotion:
          return GetEmotionInput();

        case MotionInputType.Hand:
          return GetHandInput;

        default:
          return 0;
      }
    }

    public int GetEmotionInput() {
      if (EmotionManager.Instance ==  null)
        return 0;

      return EmotionManager.Instance.GetEmotion();
    }

    public int GetHandInput() {
      if (SelectHandsManager.Instance == null)
          return 0;

      return SelectHandsManager.Instance.GetHandCode();
    }

    public void UseEmotionMode() {
      inputMode =  MotionInputType.Emotion;
    }

    public void UseHandMode() {
      inputMode =  MotionInputType.Hand;
    }
  }
}

      
