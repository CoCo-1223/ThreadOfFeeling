using SQLite;
using UnityEngine;

/// <summary>
/// 4. Story - 동화 정보
/// </summary>
[Table("Story")]
public class Story {

    // story_id, INTEGER, 동화 ID
    [PrimaryKey, AutoIncrement]
    public int story_id { get; set; } // 기본키 

    // story_title, TEXT, 동화 제목
    public string story_title { get; set; }

    // story_description, TEXT, 간단 설명
    public string story_description { get; set; }

    // story_tag, TEXT, 태그
    public string story_tag { get; set; }

    // story_reward, INTEGER, 리워드 아이템 ID
    public int story_reward { get; set; } // 외래키 -> RewardItem(item_id)

    // scenarios (시나리오 리스트)는 DB 관계(Relationship)로 처리합니다.
}