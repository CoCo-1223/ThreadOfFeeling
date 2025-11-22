using Components;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

        // [핵심 변수] 방에 있던 가구를 옮기는 중인가?
        private bool isMovingExistingItem = false;

        protected void Awake()
        {
            if (itemsContainer == null)
            {
                GameObject room = GameObject.Find("Room");
                if (room != null) 
                {
                    itemsContainer = room.transform.Find("ItemsContainer");
                    if (itemsContainer == null)
                    {
                        GameObject container = new GameObject("ItemsContainer");
                        container.transform.SetParent(room.transform);
                        itemsContainer = container.transform;
                    }
                }
            }
        }

        protected override void Start() 
        {
            if (saveLayoutButton != null) 
            {
                saveLayoutButton.onClick.RemoveAllListeners();
                saveLayoutButton.onClick.AddListener(OnSaveLayout);
            }
            
            if (exitButton != null) 
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitAndSave);
            }

            if (mainCamera == null) mainCamera = Camera.main;
            
            if (inventoryPanel != null) inventoryPanel.SetActive(true);
            
            LoadLayout();
        }

        private void OnApplicationQuit() { SaveLayout(); }
        private void OnDestroy() { SaveLayout(); }

        protected override void Update() 
        {   
            base.Update();

            if (isPlacing && currentPicture != null && mainCamera != null)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 10f;
                currentPicture.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
                
                placingFrameCount++;

                if (placingFrameCount >= 2) 
                {
                    // 클릭 -> 배치
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            PlacePicture();
                        }
                    }
                    
                    // ESC -> 취소
                    if (Input.GetKeyDown(KeyCode.Escape)) 
                    {
                        CancelPlacement();
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        DetectFurnitureClick();
                    }
                }
            }
        }

        private void DetectFurnitureClick()
        {
            if (mainCamera == null) return;
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                PlacedPicture clickedPic = hit.collider.GetComponent<PlacedPicture>();
                if (clickedPic != null)
                {
                    StartMovingPicture(clickedPic);
                }
            }
        }

        private void OnExitAndSave()
        {
            SaveLayout();
            SceneManager.LoadScene("VillageScene"); 
        }

        // [상황 A] 인벤토리에서 꺼낼 때
        public void StartPlacement(Item item) 
        {
            if (item == null || item.itemPrefab == null) return;
            if (isPlacing) CancelPlacement();

            isMovingExistingItem = false; // "새 아이템"임

            currentItem = item;
            currentPicture = Instantiate(item.itemPrefab);
            if (itemsContainer != null) currentPicture.transform.SetParent(itemsContainer, false);
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            if(mainCamera != null) currentPicture.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 2000;
            
            Collider2D col = currentPicture.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            MakeTransparent();
            isPlacing = true;
            placingFrameCount = 0;
        }

        // [상황 B] 방에 있는 거 집을 때
        public void StartMovingPicture(PlacedPicture picture)
        {
            isMovingExistingItem = true; // "기존 아이템"임

            currentItem = picture.item;
            
            // 1. 부모 끊고 삭제 (방에서 사라짐)
            picture.transform.SetParent(null);
            Destroy(picture.gameObject);
            
            // 2. ★★★ 중요: 인벤토리에 추가 안 함 (환불 X) ★★★
            // 아직 배치 중이니까 인벤토리에 넣지 않음.
            
            // 3. 레이아웃 저장 (방 데이터에서는 빠짐)
            SaveLayout();

            // 4. ★★★ 중요: 인벤토리 UI 갱신 안 함 ★★★
            // 그래야 사용자는 "아직 인벤토리에 안 들어갔구나"라고 느낌

            // 마우스에 새 가구 붙이기
            StartPlacementInternal(currentItem);
        }

        // StartMovingPicture용 내부 함수 (플래그 초기화 안 함)
        private void StartPlacementInternal(Item item)
        {
            currentPicture = Instantiate(item.itemPrefab);
            if (itemsContainer != null) currentPicture.transform.SetParent(itemsContainer, false);
            
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            if(mainCamera != null) currentPicture.transform.position = mainCamera.ScreenToWorldPoint(mousePos);
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 2000;
            
            Collider2D col = currentPicture.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            MakeTransparent();
            isPlacing = true;
            placingFrameCount = 0;
        }

        // [배치 확정]
        private void PlacePicture() 
        {
            if (currentPicture == null) { CancelPlacement(); return; }
            
            Vector3 pos = currentPicture.transform.position;
            pos.z = 0;
            currentPicture.transform.position = pos;
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }

            PlacedPicture placed = currentPicture.AddComponent<PlacedPicture>();
            placed.Initialize(currentItem, this);
            
            // ★ [로직 분기]
            if (isMovingExistingItem)
            {
                // 기존 거 옮긴 거면 -> 개수 변동 없음 (아무것도 안 함)
                // (집을 때 환불 안 받았으니, 놓을 때 차감 안 해도 됨)
            }
            else
            {
                // 인벤에서 꺼낸 새 거면 -> 개수 차감 (-1)
                if (DataManager.Instance != null && currentItem != null)
                {
                    DataManager.Instance.UseRewardItem(currentItem.itemId, 1);
                }
            }
            
            SaveLayout();

            // UI 갱신
            InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
            if (inventory != null) inventory.UpdateInventoryUI();

            currentPicture = null;
            currentItem = null;
            isPlacing = false;
        }

        // [취소 - ESC]
        private void CancelPlacement()
        {
            if (currentPicture != null) Destroy(currentPicture);
            
            // ★ [핵심 로직]
            if (isMovingExistingItem)
            {
                // 방에 있던 걸 들고 있다가 취소함 -> 이제 인벤토리에 넣어야 함 (환불)
                if (DataManager.Instance != null && currentItem != null)
                {
                    DataManager.Instance.AddRewardItem(currentItem, 1);
                    Debug.Log($"[Housing] 배치 취소(ESC): {currentItem.itemName} 인벤토리로 돌아감");
                }
            }
            else
            {
                // 인벤토리에서 꺼내다가 취소함 -> 아직 차감 안 됐으니 그냥 놔두면 됨 (원상복구)
            }

            // UI 갱신 (이제 인벤토리에 뜸!)
            InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
            if (inventory != null) inventory.UpdateInventoryUI(); 
            
            currentPicture = null;
            currentItem = null;
            isPlacing = false;
        }

        // (휴지통 삭제 시)
        public void RemovePicture(PlacedPicture picture)
        {
            if (DataManager.Instance != null && picture.item != null)
                DataManager.Instance.AddRewardItem(picture.item, 1);

            picture.transform.SetParent(null);
            Destroy(picture.gameObject);
            
            SaveLayout();
            InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
            if (inventory != null) inventory.UpdateInventoryUI();
        }

        private void MakeTransparent()
        {
            if (currentPicture == null) return;
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 0.5f; sr.color = c; }
        }

        private void OnSaveLayout() { SaveLayout(); }
        
        private void SaveLayout()
        {
            if (itemsContainer == null || DataManager.Instance == null) return;
            
            HousingLayout layout = new HousingLayout();

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
            DataManager.Instance.SaveHousingLayout(layout);
        }
        
        private void LoadLayout()
        {
            if (DataManager.Instance == null) return;
            
            HousingLayout layout = DataManager.Instance.GetCurrentHousingLayout();
            if (layout == null || layout.furnitures == null) return;
            
            ClearAllFurnitureWithoutReturn();
            
            foreach (FurnitureData data in layout.furnitures)
            {
                Item item = FindItemByName(data.itemName);
                if (item != null && item.itemPrefab != null)
                {
                    GameObject obj = Instantiate(item.itemPrefab, itemsContainer);
                    obj.transform.position = data.position;
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sortingOrder = data.sortingOrder;
                    PlacedPicture placed = obj.AddComponent<PlacedPicture>();
                    placed.Initialize(item, this);
                }
            }
        }
        
        private void ClearAllFurnitureWithoutReturn()
        {
            if (itemsContainer == null) return;
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in itemsContainer)
            {
                if (child.GetComponent<PlacedPicture>() != null) toDestroy.Add(child.gameObject);
            }
            foreach (GameObject obj in toDestroy) Destroy(obj);
        }
        
        private Item FindItemByName(string itemName)
        {
            Item[] allItems = Resources.LoadAll<Item>("Items");
            foreach (Item item in allItems) if (item.itemName == itemName) return item;
            return null;
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
            
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.enabled = true; 
        }
        
        void Update()
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 1000 + Mathf.RoundToInt(-transform.position.y * 10);
        }
    }
}