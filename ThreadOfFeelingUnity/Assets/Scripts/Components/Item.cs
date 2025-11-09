using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "GameData/Item Data")]
public class Item : ScriptableObject
{
    [Header("기본 정보")]
    public int itemId;
    public string itemName;
    [TextArea(3, 10)]
    public string itemDescription;

    [Header("게임 내 표현")]
    public Sprite itemIcon; // 인벤토리나 UI에 표시될 아이템
    public GameObject prefab; // 하우징 씬에서 실제로 배치될 프리팹
   
    // position은 이 아이템이 하우징 씬에 배치될 때의 상태 값(Runtime Data) 나중에 저장 파일(Save File)에 저장
}