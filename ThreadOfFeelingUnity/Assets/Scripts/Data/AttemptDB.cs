using SQLite;
using UnityEngine;
using System;

/// <summary>
/// 6. Attempt - 아동의 감정 퀴즈 학습 기록(로그)
/// </summary>
[Table("Attempt")]

public class AttemptDB {
    
    // 로그 구분을 위한 고유 ID (자동 증가)
    [PrimaryKey, AutoIncrement]
    public int attempt_log_id { get; set; } // 기본키 

    // (관계) "Attempt는 어떤 ChildProfile이... 기록하는 데이터"
    [Indexed]
    public int profile_id { get; set; } // 외래키 -> ChildProfile(profile_id)

    // (관계) "Attempt는... 어떤 Question에 응답했는지를 기록"
    [Indexed]
    public int question_id { get; set; }  // 외래키 -> Question(question_id)

    // selected_option, INTEGER, 선택한 보기
    public int selected_option { get; set; }

    // is_correct, BOOLEAN
    public bool is_correct { get; set; }

    // attempt_no, INTEGER, 시도 횟수
    public int attempt_no { get; set; }

    // latency_ms, INTEGER, 답변까지 걸린 시간
    public int latency_ms { get; set; }
    
    // --- 이 시도 당시의 설정 값 스냅샷 ---
    // font_scale, INTEGER
    public int font_scale { get; set; }
    
    // tts_used, BOOLEAN
    public bool tts_used { get; set; }
    
    // dyslexia_font_used, BOOLEAN
    public bool dyslexia_font_used { get; set; }
    // ------------------------------------

    // 이 로그가 생성된 시간
    public DateTime CreatedAt { get; set; }
}