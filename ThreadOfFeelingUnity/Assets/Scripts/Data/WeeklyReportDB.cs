using UnityEngine;
using SQLite;

/// <summary>
/// 10. Weekly Report - 보호자에게 제공되는 학습 요약 리포트
/// </summary>

[Table("WeeklyReport")]
public class WeeklyReportDB {
    
    // 리포트 고유 ID
    [PrimaryKey, AutoIncrement]
    public int report_id { get; set; }
    
    // (관계) "ChildProfile 1명은 여러 개의 WeeklyReport를 받음"
    [Indexed]
    public int profile_id { get; set; }

    // report_band, TEXT, 리포트 기간 (예: "2025-W40")
    public string report_band { get; set; }

    // accuracy_stats, TEXT, 문항별 정답률 통계 (JSON 문자열로 저장 권장)
    public string accuracy_stats { get; set; }

    // weak_type, TEXT, 자주 틀린 유형
    public string weak_type { get; set; }
    
    // "지도 팁"  
    public string tip { get; set; }
}