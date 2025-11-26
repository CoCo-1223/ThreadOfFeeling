using UnityEngine;

namespace Components
{
    [System.Serializable]
    public class Question {
        [Tooltip("이 칸을 비워두면 퀴즈 없이 넘어갑니다.")]
        [TextArea(2, 5)]
        public string questionText;

        // 퀴즈(질문)용 TTS 클립
        public AudioClip questionVoice;

        public string answer1;
        public string answer2;
        [Range(0, 1)]
        [Tooltip("정답 인덱스 (0=answer1, 1=answer2)")]
        public int correctAnswerIndex = 0;

        [TextArea(2, 5)]
        public string correctFeedback;
        [TextArea(2, 5)]
        public string wrongFeedback;

        [Header("스토리 타입 설정")]
        [Tooltip("이 퀴즈가 등장할 스토리 타입")]
        public StoryType targetType; 
        [Tooltip("체크하면 모든 타입에서 등장합니다.")]
        public bool isCommon = false; 
    }
}