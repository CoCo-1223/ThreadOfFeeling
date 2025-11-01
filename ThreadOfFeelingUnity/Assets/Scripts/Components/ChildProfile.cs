using System;
public class ChildProfile {
    public int childId { get; }
    public string nickname { get; set; }
    public AgeBand ageBand { get; set; }
    public Gender gender { get; }
    public int fontScale { get; set; }
    public bool isTtsUsed { get; set; }
    public bool isDyslexiaFontUsed { get; set; }
    public int roomId { get; }
    public DateTime createdAt { get; }
    public ChildProfile(string nickName, AgeBand ageBand, Gender gender) {
        //this.ChildId = numChildId+
        this.nickname = nickName;
        this.ageBand = ageBand;
        this.gender = gender;
        this.fontScale = 100;
        this.isTtsUsed = false;
        this.isDyslexiaFontUsed = false;
        //this.createdAt = DateTime.MinValue;
    }
}