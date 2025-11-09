using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Question {
    [Tooltip("이 칸을 비워두면 퀴즈 없이 넘어갑니다.")]
    [TextArea(2, 5)]
    public string questionText;

    public string answer1;
    public string answer2;
    [Range(0, 1)]
    [Tooltip("정답 인덱스 (0=answer1, 1=answer2)")]
    public int correctAnswerIndex = 0;

    [TextArea(2, 5)]
    public string correctFeedback;
    [TextArea(2, 5)]
    public string wrongFeedback;
}