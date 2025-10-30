using System;

public enum Gender {
    Male,
    Female
}

public class ChildProfile{
    public int ChildId { get; set; }
    public string NickName { get; set; }
    public int AgeBand { get; set; }
    public Gender Gender { get; set; }
    public int FontScale { get; set; }
    public bool IsTtsUsed { get; set; }
    public bool IsDyslexiaFontUsed { get; set; }
    public int RoomId { get; set; }
    public DateTime createdAt { get; set; }
    public ChildProfile() {
        this.FontScale = 100;
        this.IsTtsUsed = false;
        this.IsDyslexiaFontUsed = false;
        this.createdAt = DateTime.MinValue;
    }
}