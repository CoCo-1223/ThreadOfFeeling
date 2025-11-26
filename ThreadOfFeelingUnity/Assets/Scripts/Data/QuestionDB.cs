using SQLite;

namespace Data
{
    /// <summary>
    /// 5. Question - 챌린지 문항 정보
    /// </summary>
    [Table("Question")]
    public class QuestionDB {
        // question_id, INTEGER, 문제 ID
        [PrimaryKey, AutoIncrement]
        public int question_id { get; set; } // 기본키

        // (관계) "Scenario 1개는 여러 개의 Question을 포함"하므로 
        // 어떤 시나리오에 속한 문제인지 ID로 연결합니다.
        [Indexed] // 이 값으로 검색을 빠르게 합니다.
        public int scene_id { get; set; } // 외래키 

        // first, TEXT, 첫번째 보기
        public string first { get; set; }

        // second, TEXT, 두번째 보기
        public string second { get; set; }

        // answer, INTEGER, 정답 (1 또는 2)
        public int answer { get; set; }

        // feedbackAnswer, TEXT, 맞췄을때 피드백
        public string feedbackAnswer { get; set; }

        // feedbackWrong, TEXT, 틀렸을때 피드백
        public string feedbackWrong { get; set; }
    }
}