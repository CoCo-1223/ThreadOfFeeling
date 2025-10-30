using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoryType {
    TypeA,
    TypeB,
    NotSelected
}

[Serializable]
public class Guardian
{
    public int GuardianId { get; set; }
    private string _consentAt;
    public List<ChildProfile> ChildProfiles { get; set; }
    public StoryType SelectedStory { get; set; }
    //public Hash hash { get; set; } // PIN 번호 저장

    public Guardian() {
        this.ChildProfiles = new List<ChildProfile>();
        this.SelectedStory = StoryType.NotSelected;
    }

    public void SetConsentNow() {
        // "o" 포맷(ISO 8601)은 나중에 다시 DateTime으로 변환하기 가장 좋습니다.
        this._consentAt = DateTime.UtcNow.ToString("o");
    }

    public DateTime GetConsentAsDateTime() {
        if (DateTime.TryParse(this._consentAt, out DateTime result)) {
            return result;
        }
        return DateTime.MinValue; // 실패 시 기본값 반환
    }
}