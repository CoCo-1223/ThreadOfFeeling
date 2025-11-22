using Components;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UI
{
    public class HousingSceneUi : SceneUI 
    {
        [Header("Housing Specific")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Button saveLayoutButton;
        [SerializeField] private Button exitButton;
        
        [Header("카메라")]
        [SerializeField] private Camera mainCamera;

        private GameObject currentPicture;
        private Item currentItem;
        private bool isPlacing = false;
        private int placingFrameCount = 0;
        
        private const string HOUSING_SAVE_KEY = "HousingLayout";

        protected override void Start() 
        {
            base.Start();
            
            if (saveLayoutButton != null)
                saveLayoutButton.onClick.AddListener(OnSaveLayout);
            
            if (exitButton != null)
                exitButton.onClick.AddListener(OnClickGoToVillage);

            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                if (mainCamera.GetComponent<Physics2DRaycaster>() == null)
                {
                    mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
                    Debug.Log("[HousingSceneUI] Physics2DRaycaster 추가됨!");
                }
            }

            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);

            if (itemsContainer == null)
            {
                GameObject room = GameObject.Find("Room");
                if (room != null)
                {
                    Transform container = room.transform.Find("ItemsContainer");
                    if (container != null)
                    {
                        itemsContainer = container;
                        Debug.Log($"[HousingSceneUI] ItemsContainer 자동으로 찾음!");
                    }
                }
            }
            
            LoadLayout();
        }

        protected override void Update() 
        {   
            base.Update();

            if (isPlacing && currentPicture != null && mainCamera != null)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                currentPicture.transform.position = worldPos;
                
                placingFrameCount++;

                if (placingFrameCount >= 2)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            PlacePicture();
                        }
                    }
                    
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        CancelPlacement();
                    }
                }
            }
        }

        public void StartPlacement(Item item) 
        {
            if (item == null || item.itemPrefab == null)
                return;
            
            // 이미 배치된 아이템인지 확인
            if (IsItemAlreadyPlaced(item.itemName))
            {
                Debug.Log($"[StartPlacement] {item.itemName}은(는) 이미 배치되어 있습니다!");
                
                // 인벤토리에 아이템 반환
                InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
                if (inventory != null)
                    inventory.ReturnItem(item);
                
                return;
            }
            
            if (isPlacing)
                CancelPlacement();

            currentItem = item;
            currentPicture = Instantiate(item.itemPrefab);
            
            if (itemsContainer != null)
            {
                currentPicture.transform.SetParent(itemsContainer, false);
            }
            
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                currentPicture.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
            }
            else
            {
                currentPicture.transform.position = Vector3.zero;
            }
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 2000;
            
            MakeTransparent();
            
            isPlacing = true;
            placingFrameCount = 0;
            
            Debug.Log($"[StartPlacement] {item.itemName} 배치 모드 시작!");
        }

        private void PlacePicture() 
        {
            if (currentPicture == null)
            {
                CancelPlacement();
                return;
            }
            
            Debug.Log("[PlacePicture] 배치 시작!");
            
            Vector3 worldPosition = currentPicture.transform.position;
            worldPosition.z = 0;
            
            if (itemsContainer != null)
            {
                currentPicture.transform.SetParent(itemsContainer, true);
            }
            
            currentPicture.transform.position = worldPosition;
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f;
                sr.color = color;
            }

            PlacedPicture placed = currentPicture.AddComponent<PlacedPicture>();
            placed.Initialize(currentItem, this);
            
            // DataManager에서 제거
            if (DataManager.Instance != null && currentItem != null)
            {
                DataManager.Instance.UseRewardItem(currentItem.itemId, 1);
                Debug.Log($"[PlacePicture] DataManager에서 제거: {currentItem.itemName}");
            }
            
            Debug.Log($"[PlacePicture] {currentItem.itemName} 배치 완료!");
            
            // 자동 저장
            SaveLayout();

            currentPicture = null;
            currentItem = null;
            isPlacing = false;
        }

        private void CancelPlacement()
        {
            if (currentPicture != null)
                Destroy(currentPicture);

            if (currentItem != null)
            {
                // 취소 시 인벤토리에 반환
                InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
                if (inventory != null)
                {
                    inventory.ReturnItem(currentItem);
                    Debug.Log($"[CancelPlacement] {currentItem.itemName} 인벤토리 반환");
                }
            }

            currentPicture = null;
            currentItem = null;
            isPlacing = false;
        }

        public void StartMovingPicture(PlacedPicture picture)
        {
            if (isPlacing)
                CancelPlacement();

            currentItem = picture.item;
            currentPicture = picture.gameObject;
            
            // PlacedPicture 컴포넌트 제거 (이동 중에는 클릭 불가)
            Destroy(picture);

            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 2000;

            MakeTransparent();
            isPlacing = true;
            placingFrameCount = 0;
            
            Debug.Log($"[StartMovingPicture] {currentItem.itemName} 이동 모드");
        }

        public void RemovePicture(PlacedPicture picture)
        {
            InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
            if (inventory != null)
            {
                inventory.ReturnItem(picture.item);
            }
            
            // DataManager에 다시 추가
            if (DataManager.Instance != null && picture.item != null)
            {
                if (!DataManager.Instance.currentProfile.Inventory.HasItem(picture.item.itemId))
                {
                    DataManager.Instance.AddRewardItem(picture.item, 1);
                    Debug.Log($"[RemovePicture] DataManager에 반환: {picture.item.itemName}");
                }
            }
            
            Destroy(picture.gameObject);
            
            // 자동 저장
            SaveLayout();
        }

        private void MakeTransparent()
        {
            if (currentPicture == null) return;

            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 0.5f;
                sr.color = color;
            }
        }

        private bool IsItemAlreadyPlaced(string itemName)
        {
            if (itemsContainer == null) return false;
            
            foreach (Transform child in itemsContainer)
            {
                PlacedPicture placed = child.GetComponent<PlacedPicture>();
                if (placed != null && placed.item != null && placed.item.itemName == itemName)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnSaveLayout()
        {
            SaveLayout();
            Debug.Log("[HousingSceneUI] 레이아웃 저장 완료!");
        }
        
        private void SaveLayout()
        {
            HousingLayout layout = new HousingLayout();
            
            if (itemsContainer != null)
            {
                foreach (Transform child in itemsContainer)
                {
                    PlacedPicture placed = child.GetComponent<PlacedPicture>();
                    if (placed != null && placed.item != null)
                    {
                        FurnitureData data = new FurnitureData
                        {
                            itemName = placed.item.itemName,
                            position = child.position,
                            sortingOrder = child.GetComponent<SpriteRenderer>()?.sortingOrder ?? 0
                        };
                        layout.furnitures.Add(data);
                    }
                }
            }
            
            string json = JsonUtility.ToJson(layout);
            PlayerPrefs.SetString(HOUSING_SAVE_KEY, json);
            PlayerPrefs.Save();
            
            Debug.Log($"[HousingSceneUI] {layout.furnitures.Count}개 가구 저장됨");
        }
        
        private void LoadLayout()
        {
            if (!PlayerPrefs.HasKey(HOUSING_SAVE_KEY))
            {
                Debug.Log("[HousingSceneUI] 저장된 레이아웃 없음");
                return;
            }
            
            string json = PlayerPrefs.GetString(HOUSING_SAVE_KEY);
            HousingLayout layout = JsonUtility.FromJson<HousingLayout>(json);
            
            if (layout == null || layout.furnitures == null)
                return;
            
            // 기존 배치 아이템 제거 (DataManager 반환 없이)
            ClearAllFurnitureWithoutReturn();
            
            HashSet<string> placedItems = new HashSet<string>();
            
            foreach (FurnitureData data in layout.furnitures)
            {
                if (placedItems.Contains(data.itemName))
                {
                    Debug.Log($"[LoadLayout] {data.itemName} 중복 스킵");
                    continue;
                }
                
                Item item = FindItemByName(data.itemName);
                if (item != null && item.itemPrefab != null)
                {
                    GameObject obj = Instantiate(item.itemPrefab, itemsContainer);
                    obj.transform.position = data.position;
                    
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sortingOrder = data.sortingOrder;
                    }
                    
                    PlacedPicture placed = obj.AddComponent<PlacedPicture>();
                    placed.Initialize(item, this);
                    
                    placedItems.Add(data.itemName);
                }
            }
            
            Debug.Log($"[HousingSceneUI] {placedItems.Count}개 가구 불러옴");
        }
        
        private void ClearAllFurnitureWithoutReturn()
        {
            if (itemsContainer == null) return;
            
            List<GameObject> toDestroy = new List<GameObject>();
            
            foreach (Transform child in itemsContainer)
            {
                PlacedPicture placed = child.GetComponent<PlacedPicture>();
                if (placed != null)
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            
            foreach (GameObject obj in toDestroy)
            {
                Destroy(obj);
            }
        }
        
        private Item FindItemByName(string itemName)
        {
            Item[] allItems = Resources.LoadAll<Item>("Items");
            foreach (Item item in allItems)
            {
                if (item.itemName == itemName)
                    return item;
            }
            return null;
        }
    }

    public class PlacedPicture : MonoBehaviour
    {
        public Item item;
        private HousingSceneUi housingUI;
        private SpriteRenderer spriteRenderer;

        public void Initialize(Item pictureItem, HousingSceneUi ui)
        {
            item = pictureItem;
            housingUI = ui;
            spriteRenderer = GetComponent<SpriteRenderer>();

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        void Update()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 1000 + Mathf.RoundToInt(-transform.position.y * 10);
            }
        }

        private void OnMouseDown()
        {
            Debug.Log($"[PlacedPicture] {item.itemName} 클릭됨!");
            housingUI.StartMovingPicture(this);
        }
    }
    
    [System.Serializable]
    public class FurnitureData
    {
        public string itemName;
        public Vector3 position;
        public int sortingOrder;
    }

    [System.Serializable]
    public class HousingLayout
    {
        public List<FurnitureData> furnitures = new List<FurnitureData>();
    }
}