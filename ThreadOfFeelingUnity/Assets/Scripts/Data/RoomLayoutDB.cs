using System;
using SQLite;

namespace Data
{
    /// <summary>
    /// 9. RoomLayout - 집 꾸미기(하우징) 배치 정보
    /// </summary>

    [Table("RoomLayout")]
    public class RoomLayoutDB {
    
        // room_id, INTEGER, 방 배치 ID
        [PrimaryKey, AutoIncrement]
        public int room_id { get; set; } // 기본키

        // (관계) "ChildProfile 1명은 여러 개의 RoomLayout을 가짐"
        [Indexed]
        public int profile_id { get; set; } // 외래키 -> ChildProfile(profile_id)

        // save_time, DATETIME, 저장 시간
        public DateTime save_time { get; set; }

        // "배치 좌표/회전/크기 정보 (slots_jason)"
        // JSON 문자열(TEXT)로 저장
        public string slots_jason { get; set; }
    }
}