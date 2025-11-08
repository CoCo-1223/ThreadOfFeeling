using System;

public enum Gender {
    Male,
    Female
}

public enum AgeBand {
    Unknown = 0,            // 알 수 없음 (기본값)
    Kindergarten = 1,       // 유치원
    ElementaryLower = 2,    // 초등학교 저학년
    ElementaryUpper = 3     // 초등학교 고학년 이상
}

public enum StoryType {
    TypeA,
    TypeB,
    NotSelected
}

public enum GameState {
    Village,    // 마을
    Selection,  // 동화 선택
    Story,      // 동화
    Housing,    // 하우징
    Paused,     // 일시정지 (UI 메뉴 등)
    Loading,    // 로딩 중
}