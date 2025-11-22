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
        public void Clear() { item = null; quantity = 0; }
    }

    public class InventoryUI : MonoBehaviour
    {
        [Header("슬롯 참조")]
        [SerializeField] private Transform slotsContainer;
        
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
            // 1. 참조 연결
            if (housingSceneUI == null) housingSceneUI = FindFirstObjectByType<HousingSceneUi>();
            
            if (itemsContainer == null)
            {
                GameObject room = GameObject.Find("Room");
                if (room != null) itemsContainer = room.transform.Find("ItemsContainer");
            }
            
            if (tooltipPanel != null)
            {
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                tooltipPanel.SetActive(false);
            }
            
            // 2. 초기화
            InitializeInventory();
            
            // 3. 강제로 아이템 로드
            LoadInventoryStrict();
        }

        private void Update()
        {
            if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                Vector2 position = Input.mousePosition;
                tooltipRect.position = position;
            }
        }

        private void InitializeInventory()
        {
            inventorySlots.Clear();
            slotUIList.Clear();
            for (int i = 0; i < slotsContainer.childCount; i++)
            {
                Transform slotTransform = slotsContainer.GetChild(i);
                InventorySlotUI slotUI = slotTransform.GetComponent<InventorySlotUI>();
                if (slotUI == null) slotUI = slotTransform.gameObject.AddComponent<InventorySlotUI>();
                slotUI.Initialize(i, this);
                slotUIList.Add(slotUI);
                inventorySlots.Add(new InventorySlotData());
            }
        }

        // 외부에서 호출 시 사용
        public void UpdateInventoryUI()
        {
            LoadInventoryStrict();
        }

        // [핵심 수정] 소지 개수 0개여도 방에 없으면 무조건 표시
        private void LoadInventoryStrict()
        {
            // 1. 슬롯 비우기
            foreach (var slot in inventorySlots) slot.Clear();

            // 2. DataManager에서 현재 배치된 가구 목록 가져오기
            HashSet<string> placedItemNames = new HashSet<string>();
            if (DataManager.Instance != null)
            {
                var layout = DataManager.Instance.GetCurrentHousingLayout();
                if (layout != null && layout.furnitures != null)
                {
                    foreach (var furniture in layout.furnitures)
                    {
                        placedItemNames.Add(furniture.itemName);
                    }
                }
            }

            // 3. 폴더에 있는 모든 아이템 가져오기
            Item[] allItems = Resources.LoadAll<Item>("Items");
            int slotIndex = 0;

            foreach (Item resItem in allItems)
            {
                if (slotIndex >= inventorySlots.Count) break;

                // (A) Reward 타입인지 확인
                // 만약 아이템이 안 뜨면 Inspector에서 ItemType이 'Reward'인지 꼭 확인하세요!
                if (resItem.itemType != Item.ItemType.Reward) 
                    continue;

                // (B) 방에 배치되어 있는지 확인
                bool isPlaced = placedItemNames.Contains(resItem.itemName);

                if (isPlaced)
                {
                    // 배치됨 -> 인벤토리에서 숨김 (데이터 매니저 수량 0으로 맞춤)
                    if (DataManager.Instance != null)
                    {
                        int currentQty = DataManager.Instance.currentProfile.Inventory.GetItemCount(resItem.itemId);
                        if (currentQty > 0) DataManager.Instance.UseRewardItem(resItem.itemId, currentQty);
                    }
                }
                else
                {
                    // ★ [중요] 배치 안 됨 -> 인벤토리에 무조건 1개 표시
                    inventorySlots[slotIndex].item = resItem;
                    inventorySlots[slotIndex].quantity = 1;

                    // 데이터 매니저에도 1개 있다고 강제 주입 (싱크 맞추기)
                    if (DataManager.Instance != null)
                    {
                        int currentQty = DataManager.Instance.currentProfile.Inventory.GetItemCount(resItem.itemId);
                        if (currentQty == 0) DataManager.Instance.AddRewardItem(resItem, 1);
                    }

                    slotIndex++;
                }
            }

            // 4. UI 갱신
            for (int i = 0; i < slotUIList.Count; i++) UpdateSlotUI(i);
            
            // 변경된 데이터 저장
            if (DataManager.Instance != null) DataManager.Instance.SaveProfileData();
        }

        public void StartItemPlacement(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventorySlots.Count || inventorySlots[slotIndex].IsEmpty) return;
            
            Item selectedItem = inventorySlots[slotIndex].item;
            
            inventorySlots[slotIndex].Clear();
            UpdateSlotUI(slotIndex);
            
            if (housingSceneUI != null) housingSceneUI.StartPlacement(selectedItem);
        }

        public void ReturnItem(Item item)
        {
            LoadInventoryStrict();
        }

        private void UpdateSlotUI(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slotUIList.Count)
                slotUIList[slotIndex].UpdateDisplay(inventorySlots[slotIndex]);
        }
        
        public InventorySlotData GetSlotData(int slotIndex) => (slotIndex >= 0 && slotIndex < inventorySlots.Count) ? inventorySlots[slotIndex] : null;

        public void ShowTooltip(Item item)
        {
            if (tooltipPanel != null && tooltipText != null && item != null)
            {
                tooltipPanel.SetActive(true);
                tooltipText.text = $"<b>{item.itemName}</b>\n{item.itemExplain}";
            }
        }

        public void HideTooltip() { if (tooltipPanel != null) tooltipPanel.SetActive(false); }
    }
    
    // UI 클래스 유지
    public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Image itemIcon;
        private Image slotImage;
        private int slotIndex;
        private InventoryUI inventoryUI;
        private GameObject dragPreview;
        private Canvas canvas;
        private Coroutine tooltipCoroutine;
        private bool isDragging = false;

        public void Initialize(int index, InventoryUI inventory)
        {
            slotIndex = index;
            inventoryUI = inventory;
            canvas = GetComponentInParent<Canvas>();
            slotImage = GetComponent<Image>();
            if (slotImage != null) slotImage.raycastTarget = true;
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform != null) itemIcon = iconTransform.GetComponent<Image>();
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
            if (itemIcon == null) return;
            if (slotData.IsEmpty) { itemIcon.sprite = null; itemIcon.color = new Color(1, 1, 1, 0); }
            else { itemIcon.sprite = slotData.item.itemIcon; itemIcon.color = new Color(1, 1, 1, 1); }
        }

        public void OnPointerEnter(PointerEventData eventData) { if(!isDragging) tooltipCoroutine = StartCoroutine(ShowTooltipDelayed()); }
        public void OnPointerExit(PointerEventData eventData) { if(tooltipCoroutine!=null) StopCoroutine(tooltipCoroutine); inventoryUI.HideTooltip(); }
        private System.Collections.IEnumerator ShowTooltipDelayed() {
            yield return new WaitForSeconds(0.5f);
            if(!isDragging) {
                InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
                if (slotData != null && !slotData.IsEmpty) inventoryUI.ShowTooltip(slotData.item);
            }
        }
        public void OnBeginDrag(PointerEventData eventData) {
            isDragging = true; inventoryUI.HideTooltip();
            InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
            if (slotData == null || slotData.IsEmpty) return;
            dragPreview = new GameObject("DragPreview");
            dragPreview.transform.SetParent(canvas.transform, false);
            Image img = dragPreview.AddComponent<Image>();
            img.sprite = slotData.item.itemIcon;
            img.raycastTarget = false;
            dragPreview.GetComponent<RectTransform>().sizeDelta = new Vector2(50,50);
        }
        public void OnDrag(PointerEventData eventData) { if(dragPreview!=null) dragPreview.transform.position = eventData.position; }
        public void OnEndDrag(PointerEventData eventData) {
            isDragging = false; if(dragPreview!=null) Destroy(dragPreview);
            InventorySlotData slotData = inventoryUI.GetSlotData(slotIndex);
            if(slotData!=null && !slotData.IsEmpty) inventoryUI.StartItemPlacement(slotIndex);
        }
    }
}