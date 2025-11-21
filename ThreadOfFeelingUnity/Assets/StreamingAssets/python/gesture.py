from __future__ import annotations
from dataclasses import dataclass
from typing import Optional, Dict
import math

# MediaPipe Hands 랜드마크 인덱스
WRIST = 0

THUMB_CMC = 1
THUMB_MCP = 2
THUMB_IP  = 3
THUMB_TIP = 4

INDEX_MCP = 5
INDEX_PIP = 6
INDEX_DIP = 7
INDEX_TIP = 8

MIDDLE_MCP = 9
MIDDLE_TIP = 12

RING_MCP = 13
RING_TIP = 16

PINKY_MCP = 17
PINKY_TIP = 20

FINGER_BASES = {
    "index": INDEX_MCP,
    "middle": MIDDLE_MCP,
    "ring": RING_MCP,
    "pinky": PINKY_MCP,
}
FINGER_TIPS = {
    "index": INDEX_TIP,
    "middle": MIDDLE_TIP,
    "ring": RING_TIP,
    "pinky": PINKY_TIP,
}


@dataclass
class HandGeometry:
    # 각 손가락 curl 비율 (정규화 tip-base 거리)
    curl_ratio: Dict[str, float]
    # 각 손가락 접힘/펴짐 판정
    is_closed: Dict[str, bool]
    is_open: Dict[str, bool]
    # 전체 판정 (네 손가락 기준)
    all_closed: bool
    all_open: bool
    # 방향 특징
    thumb_up: bool     # 엄지 위 방향
    index_down: bool   # 검지 아래 방향


@dataclass
class FrameFeatures:
    width: int
    height: int
    left: Optional[HandGeometry]
    right: Optional[HandGeometry]


def _as_seq(lm_container):  # MediaPipe NormalizedLandmarkList 또는 list를 공통 인터페이스로.
    if lm_container is None:
        return None
    if hasattr(lm_container, "landmark"):
        return lm_container.landmark
    return lm_container


def _dist2d(a, b) -> float:
    return math.hypot(a[0] - b[0], a[1] - b[1])


class GestureExtractor:

    def from_mediapipe(self, results, frame_width: int, frame_height: int) -> FrameFeatures:    # 손 크기 정규화
        # MediaPipe Holistic 결과 사용
        left_lm = getattr(results, "left_hand_landmarks", None)
        right_lm = getattr(results, "right_hand_landmarks", None)

        left = self._hand_from_landmarks(left_lm)
        right = self._hand_from_landmarks(right_lm)

        return FrameFeatures(width=frame_width, height=frame_height, left=left, right=right)

    def _hand_from_landmarks(self, hand_lms) -> Optional[HandGeometry]: # 손가락 Curl 비율 계산
        if hand_lms is None:
            return None

        lm = _as_seq(hand_lms)

        # 손목 좌표(정규화 0~1)
        wrist = lm[WRIST]
        wrist_xy = (wrist.x, wrist.y)

        # 손 크기: 손목-중지 MCP 거리
        middle_mcp = lm[MIDDLE_MCP]
        hand_size = _dist2d(wrist_xy, (middle_mcp.x, middle_mcp.y))
        if hand_size < 1e-5:
            hand_size = 1e-5  # 0 division 방지

        curl_ratio: Dict[str, float] = {}
        is_closed: Dict[str, bool] = {}
        is_open: Dict[str, bool] = {}

        # 각 손가락 tip-base 거리 / hand_size
        for name in FINGER_BASES.keys():
            base_idx = FINGER_BASES[name]
            tip_idx = FINGER_TIPS[name]
            base = lm[base_idx]
            tip = lm[tip_idx]
            d = _dist2d((base.x, base.y), (tip.x, tip.y))
            ratio = d / hand_size
            curl_ratio[name] = ratio

            # 굽힘 판정 (0.45 / 0.65 임계값)
            is_closed[name] = ratio < 0.45   # 접힘
            is_open[name]   = ratio > 0.65   # 펴짐

        all_closed = all(is_closed.values())
        all_open   = all(is_open.values())

        # 방향 판정: 엄지 up, 검지 down

        # 엄지: MCP -> TIP 벡터 기준
        thumb_mcp = lm[THUMB_MCP]
        thumb_tip = lm[THUMB_TIP]
        dx_t = thumb_tip.x - thumb_mcp.x
        dy_t = thumb_tip.y - thumb_mcp.y
        # dy < 0 이고 수직 성분이 충분히 크면 위
        thumb_up = (dy_t < -0.25 * hand_size) and (abs(dx_t) < 0.8 * abs(dy_t))

        # 검지: MCP -> TIP 벡터 기준
        index_mcp = lm[INDEX_MCP]
        index_tip = lm[INDEX_TIP]
        dx_i = index_tip.x - index_mcp.x
        dy_i = index_tip.y - index_mcp.y
        # dy > 0 이고 수직 성분이 충분히 크면 아래
        index_down = (dy_i > 0.25 * hand_size) and (abs(dx_i) < 0.6 * abs(dy_i))

        return HandGeometry(
            curl_ratio=curl_ratio,
            is_closed=is_closed,
            is_open=is_open,
            all_closed=all_closed,
            all_open=all_open,
            thumb_up=thumb_up,
            index_down=index_down,
        )
