using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Guardian {
    public int GuardianId { get; set; }
    private string _consentAt;
    public List<ChildProfile> ChildProfiles { get; set; }
    //public StoryType SelectedStory { get; set; }
    //public Hash hash { get; set; } // PIN 번호 저장
    public Guardian() {
        this.ChildProfiles = new List<ChildProfile>();
        //this.SelectedStory = StoryType.NotSelected;
    }

    public void SetConsentNow() {
        this._consentAt = DateTime.UtcNow.ToString("o"); // UTC 표준
    }

    public DateTime GetConsentAsDateTime() {
        if (DateTime.TryParse(this._consentAt, out DateTime result)) {
            return result;
        }
        return DateTime.MinValue; // 실패 시 기본값 반환
    }
}