using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Components;

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
            LoadAvailableItems();
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

        private void LoadAvailableItems()
        {
            Item[] allItems = Resources.LoadAll<Item>("Items");
            
            Debug.Log($"[InventoryUI] 전체 아이템: {allItems.Length}개");

            HashSet<string> placedItemNames = new HashSet<string>();
            
            if (itemsContainer != null)
            {
                foreach (Transform child in itemsContainer)
                {
                    string itemName = child.name.Replace("(Clone)", "").Replace("_0", "").Trim();
                    placedItemNames.Add(itemName);
                }
            }

            int addedCount = 0;
            foreach (Item item in allItems)
            {
                if (item.itemType != Item.ItemType.Reward)
                    continue;

                string prefabName = item.itemPrefab != null ? item.itemPrefab.name : "";
                
                if (!placedItemNames.Contains(prefabName) && !placedItemNames.Contains(item.itemName))
                {
                    if (AddItem(item))
                    {
                        addedCount++;
                        Debug.Log($"[InventoryUI] 인벤토리에 추가: {item.itemName}");
                    }
                }
            }
            
            Debug.Log($"[InventoryUI] 인벤토리에 {addedCount}개 아이템 추가됨");
        }

        public bool AddItem(Item item)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].IsEmpty)
                {
                    inventorySlots[i].item = item;
                    inventorySlots[i].quantity = 1;
                    
                    UpdateSlotUI(i);
                    return true;
                }
            }
            
            return false;
        }

        public bool RemoveItem(Item item)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty && inventorySlots[i].item == item)
                {
                    inventorySlots[i].Clear();
                    UpdateSlotUI(i);
                    return true;
                }
            }
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
            if (inventorySlots[slotIndex].IsEmpty)
                return;

            Item selectedItem = inventorySlots[slotIndex].item;

            if (housingSceneUI != null)
            {
                housingSceneUI.StartPlacement(selectedItem);
                RemoveItem(selectedItem);
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
            AddItem(item);
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
            if (slotData != null && !slotData.IsEmpty)
            {
                inventoryUI.StartItemPlacement(slotIndex);
            }
        }
    }
}