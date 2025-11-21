using System.Collections.Generic;
using Components;
using UnityEngine;

namespace Utils {
    public static class ChecklistParser {
        // 변환 목표값 (예: 100점 만점 기준으로 환산)
        private const float TARGET_STAT_MAX = 100f;

        public static ChildStats ParseJsonToStats(string jsonString) {
            ChildStats newStats = new ChildStats();

            try {
                Dictionary<string, int> responses = ParseSimpleJson(jsonString);

                if (responses == null || responses.Count == 0) {
                    Debug.LogError("[ChecklistParser] 'responses' 데이터를 찾을 수 없거나 비어있습니다.");
                    return newStats;
                }

                // 점수 집계 (1~7번 능력치)
                float[] sums = new float[8]; 

                foreach (var kvp in responses) {
                    string key = kvp.Key; // 예: "1-1-1"
                    int score = kvp.Value; // 예: 3

                    string[] parts = key.Split('-');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int categoryIdx)) {
                        if (categoryIdx >= 1 && categoryIdx <= 7) {
                            sums[categoryIdx] += score;
                        }
                    }
                }

                // 각 능력치별 최대 점수를 다르게 적용
                // (현재 획득 점수 / 최대 가능 점수) * 100
                newStats.One   = CalculateStat(sums[1], 27f); // One은 27점 만점
                newStats.Two   = CalculateStat(sums[2], 18f); // 나머지는 18점 만점
                newStats.Three = CalculateStat(sums[3], 18f);
                newStats.Four  = CalculateStat(sums[4], 18f);
                newStats.Five  = CalculateStat(sums[5], 18f);
                newStats.Six   = CalculateStat(sums[6], 18f);
                newStats.Seven = CalculateStat(sums[7], 18f);

                Debug.Log("[ChecklistParser] 능력치 변환 완료!");
            }
            catch (System.Exception e) {
                Debug.LogError($"[ChecklistParser] 파싱 오류: {e.Message}");
            }

            return newStats;
        }

        private static Dictionary<string, int> ParseSimpleJson(string json) {
            var result = new Dictionary<string, int>();

            int startKeyIndex = json.IndexOf("\"responses\"");
            if (startKeyIndex == -1) return result;

            int openBraceIndex = json.IndexOf('{', startKeyIndex);
            if (openBraceIndex == -1) return result;

            int closeBraceIndex = json.IndexOf('}', openBraceIndex);
            if (closeBraceIndex == -1) return result;

            string content = json.Substring(openBraceIndex + 1, closeBraceIndex - openBraceIndex - 1);

            string[] pairs = content.Split(',');
            foreach (string pair in pairs) {
                string[] parts = pair.Split(':');
                if (parts.Length == 2) {
                    string key = parts[0].Trim().Replace("\"", "");
                    string valueStr = parts[1].Trim();

                    if (int.TryParse(valueStr, out int value)) {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        // 합계(sum)와 해당 카테고리의 만점(maxScore)을 받아서 100점 기준으로 환산
        private static float CalculateStat(float sum, float maxScore) {
            if (maxScore == 0) return 0;
            // 공식: (내 점수 / 만점) * 100
            float result = (sum / maxScore) * TARGET_STAT_MAX;
            // 혹시 모를 오버플로우 방지 (100점 넘지 않게)
            return Mathf.Clamp(result, 0f, TARGET_STAT_MAX);
        }
    }
}