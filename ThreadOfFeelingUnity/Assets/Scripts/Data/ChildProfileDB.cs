using System;
using SQLite;
// DateTime을 사용하기 위해 필요 

namespace Data
{
    /// <summary>
    /// 1. ChildProfile - 아동 사용자 계정 정보를 저장하는 테이블 
    /// </summary>
    [Table("ChildProfile")]
    public class ChildProfileDB
    {
        // profile_id, INTEGER, 프로필ID, p_001
        // [PrimaryKey]: 이 컬럼을 기본 키로 설정 
        // [AutoIncrement]: 데이터를 추가할 때마다 이 값을 자동으로 1씩 증가시킴 
        [PrimaryKey, AutoIncrement]
        public int profile_id { get; set; }
        // nickname, TEXT, 닉네임
        public string nickname { get; set; }

        // age_band, TEXT, 연령대 (예: "8~10")
        public string age_band { get; set; }

        // font_scale, INTEGER, 폰트 크기
        public int font_scale { get; set; }

        // tts_used, BOOLEAN, TTS 사용 여부
        public bool tts_used { get; set; }

        // dyslexia_font_used, BOOLEAN, 난독증 폰트 사용 여부
        public bool dyslexia_font_used { get; set; }

        // room_id, INTEGER, 하우징 방 ID
        public int room_id { get; set; }

        // created_at, DATETIME, 프로필 생성 시간
        public DateTime created_at { get; set; }

        // gender, TEXT (요구사항의 'Gender' 타입을 TEXT로 처리)
        public string gender { get; set; }

        // 어떤 보호자(Guardian)에게 속해있는지 연결하는 '외래 키'
        [Indexed]
        public int guardian_id { get; set; }
    }
}
