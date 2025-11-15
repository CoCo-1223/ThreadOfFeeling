using SQLite;

// <summary>
namespace Data
{
    /// 8. Inventory - 아동이 보유한 보상 아이템 현황 (N:M 관계 테이블)
    /// "각 아동(profile_id)이 특정 아이템(item_id)을 몇 개(qty) 가졌는지" 저장
    /// </summary>

    [Table("Inventory")]
    public class InventoryDB {
    
        // 인벤토리 항목 고유 ID
        [PrimaryKey, AutoIncrement]
        public int inventory_id { get; set; } // 기본키 

        // (관계) 어떤 아동의 인벤토리인지
        [Indexed]
        public int profile_id { get; set; } // 외래키 -> ChildProfile(profile_id)

        // (관계) 어떤 아이템을 가지고 있는지
        [Indexed]
        public int item_id { get; set; } // 외래키 -> RewardItem(item_id)

        // qty, INTEGER, 아이템 수
        public int qty { get; set; }
    }
}