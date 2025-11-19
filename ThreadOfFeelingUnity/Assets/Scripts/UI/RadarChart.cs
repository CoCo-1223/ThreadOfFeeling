using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 텍스트 사용을 위해 필수

namespace UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RadarChart : MaskableGraphic {
        [Header("Chart Settings")]
        [Tooltip("그래프의 반지름 (크기)")]
        [SerializeField] private float radius = 150f;
        [Tooltip("선 두께 (색 채우기 대신 선만 그림)")]
        [SerializeField] private float thickness = 5f; 
        [Tooltip("능력치 개수 (7각형)")]
        [SerializeField] private int statCount = 7;
        
        [Header("Label Settings")]
        [Tooltip("꼭짓점에 표시할 숫자 텍스트 프리팹 (TMP)")]
        [SerializeField] private TextMeshProUGUI labelPrefab;
        [Tooltip("숫자 라벨이 그래프 끝(100%)에서 얼마나 더 떨어져 있을지")]
        [SerializeField] private float labelDistance = 20f;

        // 0.0 ~ 1.0 사이로 정규화된 데이터 배열
        private float[] statValues;
        private List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();

        protected override void Awake() {
            base.Awake();
        }

        // 오브젝트가 켜질 때 라벨 생성
        protected override void OnEnable() {
            base.OnEnable();
            //CreateLabels();
        }

        // 오브젝트가 꺼질 때 라벨 삭제 (에디터 찌꺼기 방지)
        protected override void OnDisable() {
            base.OnDisable();
            //ClearLabels();
        }

        public void SetStats(float[] normalizedValues) {
            if (normalizedValues == null || normalizedValues.Length != statCount) {
                this.statValues = new float[statCount];
            } else {
                this.statValues = normalizedValues;
            }

            // [테스트용] 데이터가 0일 때 최대 크기로 표시
            bool allZero = true;
            foreach(float v in this.statValues) if(v > 0.01f) allZero = false;
            
            if(allZero) {
                 for(int i=0; i<statCount; i++) this.statValues[i] = 0.99f; 
            }

            SetVerticesDirty();
        }

        private void ClearLabels() {
            // 1. 리스트에 있는 라벨 삭제
            foreach(var label in labels) {
                if(label != null) {
                    if (Application.isPlaying) Destroy(label.gameObject);
                    else DestroyImmediate(label.gameObject);
                }
            }
            labels.Clear();

            // 2. 안전장치: 하위 오브젝트 중 이름이 Label_로 시작하는 것들 모두 삭제
            // (리스트 참조를 잃어버렸을 때를 대비함)
            List<GameObject> toDestroy = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Label_")) {
                    toDestroy.Add(child.gameObject);
                }
            }

            foreach (GameObject g in toDestroy) {
                if (Application.isPlaying) Destroy(g);
                else DestroyImmediate(g);
            }
        }

        private void CreateLabels() {
            if (labelPrefab == null) return;

            ClearLabels(); // 기존 라벨 정리

            float angleStep = 360f / statCount;
            
            for (int i = 0; i < statCount; i++) {
                TextMeshProUGUI newLabel = Instantiate(labelPrefab, transform);
                newLabel.name = $"Label_{i+1}";
                newLabel.text = (i + 1).ToString(); 
                
                // 스케일 및 회전 초기화
                newLabel.transform.localScale = Vector3.one;
                newLabel.transform.localRotation = Quaternion.identity;
                
                // Z축 0으로 고정
                Vector3 localPos = newLabel.transform.localPosition;
                localPos.z = 0f;
                newLabel.transform.localPosition = localPos;

                // 텍스트 중앙 정렬
                newLabel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                newLabel.alignment = TextAlignmentOptions.Center;
                
                // [위치 계산]
                // (i * -angleStep + 90): 12시 방향(90도)부터 시작하여 시계방향으로 회전
                float angle = (i * -angleStep + 90) * Mathf.Deg2Rad;
                
                // 반지름(100% 지점) + 여유 거리
                float dist = radius + labelDistance;
                
                float x = Mathf.Cos(angle) * dist;
                float y = Mathf.Sin(angle) * dist;
                
                newLabel.rectTransform.anchoredPosition = new Vector2(x, y);
                
                labels.Add(newLabel);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh) {
            vh.Clear();
            if (statValues == null || statValues.Length == 0) return;

            float angleStep = 360f / statCount;
            Vector2? prevPos = null;
            Vector2 firstPos = Vector2.zero;

            for (int i = 0; i < statCount; i++) {
                float angle = (i * -angleStep + 90) * Mathf.Deg2Rad;
                float currentRadius = radius * statValues[i];

                float x = Mathf.Cos(angle) * currentRadius;
                float y = Mathf.Sin(angle) * currentRadius;
                Vector2 currentPos = new Vector2(x, y);

                if (i == 0) firstPos = currentPos;

                if (prevPos.HasValue) {
                    DrawLine(vh, prevPos.Value, currentPos, thickness);
                }
                
                prevPos = currentPos;
            }

            if (prevPos.HasValue) {
                DrawLine(vh, prevPos.Value, firstPos, thickness);
            }
        }

        private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float width) {
            Vector2 dir = (end - start).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * width * 0.5f;

            UIVertex[] verts = new UIVertex[4];
            UIVertex v = UIVertex.simpleVert;
            v.color = color;

            v.position = start - normal; verts[0] = v;
            v.position = start + normal; verts[1] = v;
            v.position = end + normal;   verts[2] = v;
            v.position = end - normal;   verts[3] = v;

            vh.AddUIVertexQuad(verts);
        }
    }
}