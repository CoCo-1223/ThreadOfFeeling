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