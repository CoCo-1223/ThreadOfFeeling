using UnityEngine;

namespace Components {

    // 7각형 능력치 데이터
    [System.Serializable]
    public class ChildStats {
        // 7가지 다중 지능 예시
        public float One;         // 1. 상황 단서 기반 감정 추론 능력
        public float Two;         // 2. 행동 단서 기반 감정 추론 능력 (표정 제외)
        public float Three;       // 3. 언어·말투 단서 기반 감정 이해
        public float Four;        // 4. 사회적 맥락 및 관계 기반 감정 이해
        public float Five;        // 5. 이야기 전체 흐름 기반 감정 이해
        public float Six;         // 6. 오답 분석 및 학습 반응
        public float Seven;       // 7. 선택 과정의 인지 전략 및 참여도

        // 차트 그리기용 배열 반환
        public float[] GetNormalizedValues(float maxStatValue = 100f) {
            return new float[] {
                Mathf.Clamp01(One / maxStatValue),
                Mathf.Clamp01(Two / maxStatValue),
                Mathf.Clamp01(Three / maxStatValue),
                Mathf.Clamp01(Four / maxStatValue),
                Mathf.Clamp01(Five / maxStatValue),
                Mathf.Clamp01(Six / maxStatValue),
                Mathf.Clamp01(Seven / maxStatValue)
            };
        }

        public ChildStats() {
            // 초기값
            One = 0; Two = 0; Three = 0;
            Four = 0; Five = 0; Six = 0; Seven = 0;
        }
    }
}