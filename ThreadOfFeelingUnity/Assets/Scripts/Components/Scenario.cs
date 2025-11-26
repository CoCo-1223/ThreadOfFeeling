using UnityEngine;
using System.Collections.Generic;

namespace Components
{
    [System.Serializable]
    public class Scenario {
        [Header("기본 내용")]
        public Sprite image;

        [TextArea(3, 10)]
        public string dialogueText;

        // 시나리오용 TTS 클립
        [Header("오디오")]
        public AudioClip ttsClip;

        [Header("퀴즈 (3개)")]
        [Tooltip("이 시나리오의 퀴즈 데이터. 없으면 null로 두거나 퀴즈의 Question Text를 비워두세요.")]
        public List<Question> quizzes = new List<Question>();
    }
}