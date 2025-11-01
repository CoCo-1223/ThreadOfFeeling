using System;

public class Question {
    public int questionId {  get; } // 문제 ID
    public string first { get; }    // 첫 번째 보기
    public string second { get; }   // 두 번째 보기
    public int answer { get; }   // 정답
    public string feedbackAnswer { get; } // 맞췄을 때 피드백
    public string feedbackWrong { get; } // 맞췄을 때 피드백
    public Question() { }
}