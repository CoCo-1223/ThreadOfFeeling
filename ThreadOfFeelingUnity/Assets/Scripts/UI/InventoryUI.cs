using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Components;
using Managers;

namespace UI
{
    [System.Serializable]
    public class InventorySlotData
    {
        public Item item;
        public int quantity;
        
        public bool IsEmpty => item == null;
        
        public void Clear()
        {
            item = null;
            quantity = 0;
        }
    }

    public class InventoryUI : MonoBehaviour
    {
        [Header("슬롯 참조")]
        [SerializeField] private Transform slotsContainer;
        
        [Header("인벤토리 설정")]
        [SerializeField] private int maxSlots = 5;
        
        [Header("참조")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private HousingSceneUi housingSceneUI;
        
        [Header("툴팁")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        
        private List<InventorySlotData> inventorySlots = new List<InventorySlotData>();
        private List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();
        private RectTransform tooltipRect;

        private void Start()
        {
            if (housingSceneUI == null)
            {
                housingSceneUI = FindFirstObjectByType<HousingSceneUi>();
            }
            
            Debug.Log($"[InventoryUI] HousingSceneUI: {(housingSceneUI != null ? "찾음" : "없음")}");
            
            if (itemsContainer == null)
            {
                GameObject room = GameObject.Find("Room");
                if (room != null)
                {
                    Transform container = room.transform.Find("ItemsContainer");
                    if (container != null)
                        itemsContainer = container;
                }
            }
            
            if (tooltipPanel != null)
            {
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                tooltipPanel.SetActive(false);
            }
            
            InitializeInventory();
            
            // 중복 제거 후 로드
            CleanupDuplicateItems();
            LoadFromDataManager();
        }

        private void Update()
        {
            if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                Vector2 position = Input.mousePosition;
                
                float pivotX = position.x / Screen.width;
                float pivotY = position.y / Screen.height;
                
                tooltipRect.pivot = new Vector2(pivotX, pivotY);
                tooltipRect.position = position;
            }
        }

        private void InitializeInventory()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                inventorySlots.Add(new InventorySlotData());
            }

            FindExistingSlots();
        }

        private void FindExistingSlots()
        {
            slotUIList.Clear();

            for (int i = 0; i < slotsContainer.childCount; i++)
            {
                Transform slotTransform = slotsContainer.GetChild(i);
                
                InventorySlotUI slotUI = slotTransform.GetComponent<InventorySlotUI>();
                if (slotUI == null)
                {
                    slotUI = slotTransform.gameObject.AddComponent<InventorySlotUI>();
                }
                
                slotUI.Initialize(i, this);
                slotUIList.Add(slotUI);
            }

            maxSlots = slotUIList.Count;
        }

        // 중복 아이템 제거
        private void CleanupDuplicateItems()
        {
            if (DataManager.Instance == null || DataManager.Instance.currentProfile == null)
                return;
            
            var inventory = DataManager.Instance.currentProfile.Inventory;
            
            if (inventory == null || inventory.Slots == null || inventory.Slots.Count == 0)
            {
                // 최초 실행 시 테스트 아이템 추가
                AddTestItems();
                return;
            }
            
            // 중복된 itemId를 하나로 합치기
            Dictionary<int, int> itemTotals = new Dictionary<int, int>();
            
            foreach (var slot in inventory.Slots)
            {
                if (itemTotals.ContainsKey(slot.itemId))
                {
                    // 이미 있으면 개수만 증가 (하우징은 1개로 제한)
                    itemTotals[slot.itemId] = 1;
                }
                else
                {
                    itemTotals[slot.itemId] = 1; // 하우징 아이템은 무조건 1개
                }
            }
            
            // 인벤토리 클리어하고 다시 추가
            inventory.Slots.Clear();
            
            foreach (var kvp in itemTotals)
            {
                inventory.Slots.Add(new InventorySlot(kvp.Key, kvp.Value));
            }
            
            DataManager.Instance.SaveProfileData();
            Debug.Log($"[CleanupDuplicateItems] {itemTotals.Count}개 아이템으로 정리 완료!");
        }

        // 최초 1회만 테스트 아이템 추가
        private void AddTestItems()
        {
            Item[] allItems = Resources.LoadAll<Item>("Items");
            
            foreach (Item item in allItems)
            {
                if (item.itemType == Item.ItemType.Reward)
                {
                    if (DataManager.Instance != null && DataManager.Instance.currentProfile != null)
                    {
                        DataManager.Instance.AddRewardItem(item, 1);
                        Debug.Log($"[AddTestItems] {item.itemName} 추가");
                    }
                }
            }
        }

        // DataManager에서 인벤토리 불러오기
        private void LoadFromDataManager()
        {
            if (DataManager.Instance == null || DataManager.Instance.currentProfile == null)
            {
                Debug.LogWarning("[InventoryUI] DataManager 또는 currentProfile이 없습니다!");
                return;
            }

            if (DataManager.Instance.currentProfile.Inventory == null)
            {
                Debug.LogWarning("[InventoryUI] Inventory가 초기화되지 않았습니다!");
                return;
            }

            // 기존 슬롯 클리어
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                inventorySlots[i].Clear();
            }

            // 배치된 아이템 목록 가져오기
            HashSet<string> placedItemNames = GetPlacedItemNames();

            Item[] allItems = Resources.LoadAll<Item>("Items");
            HashSet<int> addedItemIds = new HashSet<int>(); // itemId로 중복 방지
            
            int slotIndex = 0;
            
            // 모든 Reward 타입 아이템 체크
            foreach (Item item in allItems)
            {
                if (slotIndex >= inventorySlots.Count)
                    break;
                    
                if (item.itemType == Item.ItemType.Reward)
                {
                    // 이미 슬롯에 추가된 itemId는 스킵 (중복 방지)
                    if (addedItemIds.Contains(item.itemId))
                    {
                        Debug.Log($"[LoadFromDataManager] {item.itemName} (ID:{item.itemId}) 중복 스킵");
                        continue;
                    }
                    
                    // 이미 배치된 아이템은 스킵
                    if (placedItemNames.Contains(item.itemName))
                    {
                        Debug.Log($"[LoadFromDataManager] {item.itemName}은(는) 이미 배치되어 있어 스킵");
                        continue;
                    }
                    
                    // Inventory에서 해당 아이템 수량 확인
                    int amount = DataManager.Instance.currentProfile.Inventory.GetItemCount(item.itemId);
                    
                    if (amount > 0)
                    {
                        inventorySlots[slotIndex].item = item;
                        inventorySlots[slotIndex].quantity = 1; // 하우징은 항상 1개만 표시
                        addedItemIds.Add(item.itemId); // 중복 방지를 위해 추가
                        slotIndex++;
                        
                        Debug.Log($"[LoadFromDataManager] 로드: {item.itemName} (ID: {item.itemId}, DataManager 수량: {amount})");
                    }
                }
            }

            // UI 업데이트
            for (int i = 0; i < slotUIList.Count; i++)
            {
                UpdateSlotUI(i);
            }

            Debug.Log($"[LoadFromDataManager] {slotIndex}개 아이템 로드됨 (중복 제거됨)");
        }
        
        // 배치된 아이템 목록 가져오기
        private HashSet<string> GetPlacedItemNames()
        {
            HashSet<string> placedItems = new HashSet<string>();
            
            string saveKey = "HousingLayout";
            
            if (!PlayerPrefs.HasKey(saveKey))
            {
                Debug.Log("[GetPlacedItemNames] 저장된 레이아웃 없음");
                return placedItems;
            }
            
            string json = PlayerPrefs.GetString(saveKey);
            Debug.Log($"[GetPlacedItemNames] JSON: {json}");
            
            // UI. 접두사 추가!
            UI.HousingLayout layout = JsonUtility.FromJson<UI.HousingLayout>(json);
            
            if (layout != null && layout.furnitures != null)
            {
                Debug.Log($"[GetPlacedItemNames] {layout.furnitures.Count}개 배치된 가구 발견");
                
                foreach (var furniture in layout.furnitures)
                {
                    placedItems.Add(furniture.itemName);
                    Debug.Log($"[GetPlacedItemNames] 배치됨: {furniture.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("[GetPlacedItemNames] layout 파싱 실패!");
            }
            
            return placedItems;
        }

        public bool AddItem(Item item)
        {
            // 이미 인벤토리에 있는지 확인 (중복 방지)
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item.itemId == item.itemId)
                {
                    Debug.Log($"[AddItem] {item.itemName}은(는) 이미 인벤토리에 있습니다!");
                    return false;
                }
            }

            // 빈 슬롯에 추가
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    inventorySlots[i].item = item;
                    inventorySlots[i].quantity = 1;
                    
                    // DataManager에 저장 (이미 있으면 추가 안 됨)
                    if (DataManager.Instance != null)
                    {
                        if (!DataManager.Instance.currentProfile.Inventory.HasItem(item.itemId))
                        {
                            DataManager.Instance.AddRewardItem(item, 1);
                        }
                    }
                    
                    UpdateSlotUI(i);
                    Debug.Log($"[AddItem] {item.itemName} 추가 성공 (슬롯 {i})");
                    return true;
                }
            }
            
            Debug.LogWarning($"[AddItem] 인벤토리가 가득 찼습니다! (아이템: {item.itemName})");
            return false;
        }

        public bool RemoveItem(Item item)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item.itemId == item.itemId)
                {
                    inventorySlots[i].Clear();
                    
                    // DataManager에서 제거는 배치 완료 시에만
                    // 여기서는 UI만 업데이트
                    UpdateSlotUI(i);
                    
                    Debug.Log($"[RemoveItem] 슬롯 {i}에서 {item.itemName} 제거됨");
                    return true;
                }
            }
            
            Debug.LogWarning($"[RemoveItem] {item.itemName}을 찾을 수 없습니다!");
            return false;
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slotUIList.Count)
            {
                slotUIList[slotIndex].UpdateDisplay(inventorySlots[slotIndex]);
            }
        }

        public InventorySlotData GetSlotData(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
                return inventorySlots[slotIndex];
            return null;
        }

        public void StartItemPlacement(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Count)
            {
                Debug.LogError($"[StartItemPlacement] 잘못된 슬롯 인덱스: {slotIndex}");
                return;
            }
                
            if (inventorySlots[slotIndex].IsEmpty)
            {
                Debug.LogWarning($"[StartItemPlacement] 슬롯 {slotIndex}가 비어있습니다!");
                return;
            }

            Item selectedItem = inventorySlots[slotIndex].item;
            
            Debug.Log($"[StartItemPlacement] 배치 시작: {selectedItem.itemName} (슬롯 {slotIndex})");

            if (housingSceneUI != null)
            {
                // UI 슬롯에서 먼저 제거
                inventorySlots[slotIndex].Clear();
                UpdateSlotUI(slotIndex);
                Debug.Log($"[StartItemPlacement] UI 슬롯 {slotIndex} 비움");
                
                // 배치 시작
                housingSceneUI.StartPlacement(selectedItem);
            }
            else
            {
                Debug.LogError("[StartItemPlacement] HousingSceneUI를 찾을 수 없습니다!");
            }
        }

        public void ShowTooltip(Item item)
        {
            if (tooltipPanel != null && tooltipText != null && item != null)
            {
                tooltipPanel.SetActive(true);
                tooltipText.text = $"<b>{item.itemName}</b>\n{item.itemExplain}";
            }
        }

        public void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        public void ReturnItem(Item item)
        {
            bool added = AddItem(item);
            if (!added)
            {
                // 슬롯이 가득 찬 경우, DataManager에만 반환
                if (DataManager.Instance != null && !DataManager.Instance.currentProfile.Inventory.HasItem(item.itemId))
                {
                    DataManager.Instance.AddRewardItem(item, 1);
                    Debug.Log($"[ReturnItem] {item.itemName} DataManager에만 반환 (슬롯 가득 참)");
                }
            }
        }
        
        public void RemoveItemByName(string itemName)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item.itemName == itemName)
                {
                    Item item = inventorySlots[i].item;
                    inventorySlots[i].Clear();
                    
                    UpdateSlotUI(i);
                    Debug.Log($"[RemoveItemByName] 인벤토리에서 제거: {itemName}");
                    return;
                }
            }
        }
    }

    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image itemIcon;
        private Image slotImage;
        private int slotIndex;
        private InventoryUI inventoryUI;
        private GameObject dragPreview;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private Coroutine tooltipCoroutine;
        private bool isDragging = false;

        public void Initialize(int index, InventoryUI inventory)
        {
            slotIndex = index;
            inventoryUI = inventory;
            canvas = GetComponentInParent<Canvas>();
            slotImage = GetComponent<Image>();

            if (slotImage != null)
            {
                slotImage.raycastTarget = true;
            }

            Transform iconTransform = transform.Find("ItemIcon");
            
            if (iconTransform != null)
            {
                itemIcon = iconTransform.GetComponent<Image>();
            }
            else
            {
                GameObject iconObj = new GameObject("ItemIcon");
                iconObj.transform.SetParent(transform, false);
                itemIcon = iconObj.AddComponent<Image>();
            }
            
            SetupIconRectTransform();
            itemIcon.raycastTarget = false;
            itemIcon.color = new Color(1, 1, 1, 0);
        }

        private void SetupIconRectTransform()
        {
            RectTransform iconRect = itemIcon.rectTransform;
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.offsetMin = new Vector2(5, 5);
            iconRect.offsetMax = new Vector2(-5, -5);
        }

        public void UpdateDisplay(InventorySlotData slotData)
        {
            if (itemIcon == null)
                return;

            if (slotData.IsEmpty)
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1, 1, 1, 0);
            }
            else
            {
                itemIcon.sprite = slotData.item.itemIcon;
                itemIcon.color = new Color(1, 1, 1, 1);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isDragging) return;
            
            if (tooltipCoroutine != null)
                StopCoroutine(tooltipCoroutine);
                
            tooltipCoroutine = StartCoroutine(ShowTooltipDelayed());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
            inventoryUI.HideTooltip();
        }

        private System.Collections.IEnumerator ShowTooltipDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            
            if (isDragging)
            {
                tooltipCoroutine = null;
                yield break;
            }
            
            InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
            if (slotData != null && !slotData.IsEmpty)
            {
                inventoryUI.ShowTooltip(slotData.item);
            }
            tooltipCoroutine = null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            
            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
            inventoryUI.HideTooltip();
            
            InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
            
            if (slotData == null || slotData.IsEmpty)
                return;

            dragPreview = new GameObject("DragPreview");
            dragPreview.transform.SetParent(canvas.transform, false);
            
            Image previewImage = dragPreview.AddComponent<Image>();
            previewImage.sprite = slotData.item.itemIcon;
            previewImage.raycastTarget = false;
            
            RectTransform rectTransform = dragPreview.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);
            
            canvasGroup = dragPreview.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragPreview != null)
            {
                dragPreview.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            
            if (dragPreview != null)
                Destroy(dragPreview);

            InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
            
            Debug.Log($"[OnEndDrag] 슬롯 {slotIndex}, IsEmpty: {slotData?.IsEmpty}, 아이템: {slotData?.item?.itemName}");
            
            if (slotData != null && !slotData.IsEmpty)
            {
                inventoryUI.StartItemPlacement(slotIndex);
            }
            else
            {
                Debug.LogWarning($"[OnEndDrag] 슬롯 {slotIndex}가 비어있습니다!");
            }
        }
    }

}