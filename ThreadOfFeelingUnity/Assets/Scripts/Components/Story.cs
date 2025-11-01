using System;
using System.Collections.Generic;

public class Story {
    public int storyId {  get; } // 동화 ID
    public string storyTitle { get; } // 동화 제목
    public string storyDescription {  get; } // 간단한 동화 설명
    public List<Scenario> scenarios { get; } // 연결된 시나리오들
    public string storyTag { get; } // 태그
    public Item storyReward { get; } // 스토리 보상 아이템
    public Story(int storyId, string storyTitle, string storyDescription, List<Scenario> scenarios, Item item) {
        this.storyId = storyId;
        this.storyTitle = storyTitle;
        this.storyDescription = storyDescription;
        this.scenarios = scenarios;
        this.storyReward = item;
    }
}