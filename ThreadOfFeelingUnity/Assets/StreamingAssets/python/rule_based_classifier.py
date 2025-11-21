from __future__ import annotations
from collections import Counter, deque
from dataclasses import dataclass
from typing import Optional, Deque

from gesture import FrameFeatures, HandGeometry

JOY = 'JOY'
SAD = "SAD"
ANGER = "ANGER"
DISLIKE = "DISLIKE"
NEUTRAL = "NEUTRAL"
UNKNOWN = "UNKNOWN"
HOLD = "HOLD"

@dataclass
class ClassifierConfig:
    
    # 라벨 안정화
    stab_window: int = 7           # 최근 7프레임
    stab_ratio: float = 0.6          # 60% 이상이면 확정


class RuleBasedClassifier:
    def __init__(self, cfg: Optional[ClassifierConfig] = None):
        self.cfg = cfg or ClassifierConfig()
        self.recent_labels: Deque[str] = deque(maxlen=self.cfg.stab_window)

    def _choose_hand(self, ff: FrameFeatures) -> Optional[HandGeometry]:    # 왼손/오른손 중 하나 선택.
        if ff.left is not None:
            return ff.left
        if ff.right is not None:
            return ff.right
        return None

    def infer_once(self, ff: FrameFeatures) -> str:
        hand = self._choose_hand(ff)

        if hand is None:
            base_label = UNKNOWN
        else:
            c = hand.is_closed
            o = hand.is_open
            # 정적 제스처
            # JOY: 엄지 외 나머지 모두 closed 상태 + 엄지 위
            if hand.thumb_up and c["index"] and c["middle"] and c["ring"] and c["pinky"]:
                base_label = JOY
            # SAD: 엄지, 검지 open, 나머지 closed 상태 + 검지 아래
            elif o["index"] and c["middle"] and c["ring"] and c["pinky"] and hand.index_down:
                base_label = SAD
            # ANGER: 손가락 모두 closed
            elif c["index"] and c["middle"] and c["ring"] and c["pinky"]:
                base_label = ANGER
            # DISLIKE: 손가락 모두 open
            elif o["index"] and o["middle"] and o["ring"] and o["pinky"]:
                base_label = DISLIKE
            # 나머지는 중립
            else:
                base_label = NEUTRAL

        # 라벨 안정화
        self.recent_labels.append(base_label)
        return self._stabilize_label()

    def _stabilize_label(self) -> str:
        if len(self.recent_labels) < self.recent_labels.maxlen:
            # 버퍼가 꽉 차지 않았으면 최신값 그대로
            return self.recent_labels[-1]

        cnt = Counter(self.recent_labels)
        label, c = cnt.most_common(1)[0]
        if c >= int(self.cfg.stab_window * self.cfg.stab_ratio):
            return label
        else:
            # 과반수 이하 최신값 유지
            return self.recent_labels[-1]
