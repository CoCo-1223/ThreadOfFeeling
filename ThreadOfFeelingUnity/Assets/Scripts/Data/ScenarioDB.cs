using SQLite;
using UnityEngine;

/// <summary>
/// 3. Scenario - 감정 퀴즈의 배경이 되는 상황 정보
/// </summary>
[Table("Scenario")]
public class ScenarioDB
{
    // sceneId, INTEGER, 시나리오 ID
    [PrimaryKey] // (AutoIncrement가 없으므로 "s_001"같은 값을 쓰려면 INTEGER 대신 TEXT여야 합니다. 
                 //  요구사항에 INTEGER로 되어있어 일단 숫자로 가정합니다.)
    public int scene_id { get; set; }

    // (관계) "Story 1개는 여러 개의 Scenario를 가짐"
    // 내가 어떤 Story에 속해있는지 ID로 연결
    [Indexed]
    public int story_id { get; set; }

    // sceneDescription, TEXT, 해당 시나리오 내용
    public string sceneDescription { get; set; }

    // images, INTEGER, 그림 장면 (ID 또는 순서)
    public int images { get; set; }
}