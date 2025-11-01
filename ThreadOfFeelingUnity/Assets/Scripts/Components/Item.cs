using System;
using System.Numerics;

public class Item {
    public int itemId { get; }                  // 아이템 ID
    public string itmeName { get; }             // 아이템 이름
    public string itemDescription { get; }      // 아이템 설명
    public Vector3 position { get; set; }       // 하우징 위치 벡터
    public Item(int itemId, string itemName, string itemDescroption, Vector3 position) {
        this.itemId = itemId;
        this.itmeName = itemName;
        this.itemDescription = itemDescroption;
        this.position = position;
    }
}