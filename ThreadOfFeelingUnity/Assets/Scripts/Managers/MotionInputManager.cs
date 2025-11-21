using UnityEngine;

namespace Managers{
  public enum MotionInputType {
    Emotion,
    Hand
  }

  public class MotionInputManager : MonoBehaviour{
    public static MotionInputManager Instance {get; private set; }

    public MotionInputType inputMode = MotionInputType.Emotion;

    private void Awake() {
      if (Instance != null && Instance != this) {
        Destroy(gameObject);
        return;
      }

      Instance =  this;
      DontDestroyOnLoad(gameObject);
    }

    public int GetMotionInput() {
      switch (inputMode) {
        case MotionInputType.Emotion:
          return GetEmotionInput();

        case MotionInputType.Hand:
          return GetHandInput();

        default:
          return 0;
      }
    }

    public int GetEmotionInput() {
      if (EmotionManager.Instance ==  null)
        return 0;

      return EmotionManager.Instance.GetEmotion();
    }

    private int GetHandInput() {
        if (SelectHandsManager.Instance == null) {
            return 0;
        }
        return SelectHandsManager.Instance.GetHandCode();
    }

    public MotionInputType UseEmotionMode() {
      return inputMode =  MotionInputType.Emotion;
    }

    public MotionInputType UseHandMode() {
      return inputMode =  MotionInputType.Hand;
    }

  }
}

      
