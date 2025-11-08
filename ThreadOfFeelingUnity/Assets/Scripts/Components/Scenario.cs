using System;
using UnityEngine;

[System.Serializable]
public class Scenario {
    [Header("기본 내용")]
    public Sprite image;
    [TextArea(3, 10)]
    public string dialogueText;

    [Header("퀴즈 (선택 사항)")]
    [Tooltip("이 시나리오의 퀴즈 데이터. 없으면 null로 두거나 퀴즈의 Question Text를 비워두세요.")]
    public Question quiz;
}