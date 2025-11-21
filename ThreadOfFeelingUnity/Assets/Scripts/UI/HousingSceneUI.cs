using Components;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
                else
                {
                    Debug.Log("[HousingSceneUI] Physics2DRaycaster 이미 있음!");
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
                    else
                    {
                        Debug.LogError("[HousingSceneUI] Room/ItemsContainer를 찾을 수 없습니다!");
                    }
                }
                else
                {
                    Debug.LogError("[HousingSceneUI] Room을 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.Log($"[HousingSceneUI] ItemsContainer 설정됨: {itemsContainer.name}");
            }
            
            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);
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
                        Debug.Log($"클릭 감지! UI 위인가? {EventSystem.current.IsPointerOverGameObject()}");
                        
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            PlacePicture();
                        }
                        else
                        {
                            Debug.Log("UI 위라서 무시됨");
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
            
            if (isPlacing)
                CancelPlacement();

            currentItem = item;
            currentPicture = Instantiate(item.itemPrefab);
            
            Debug.Log($"[StartPlacement] ItemsContainer: {(itemsContainer != null ? "있음" : "없음")}");
            
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
            if (currentPicture == null || itemsContainer == null)
            {
                CancelPlacement();
                return;
            }
            
            Debug.Log("[PlacePicture] 배치 시작!");
            
            Vector3 worldPosition = currentPicture.transform.position;
            worldPosition.z = 0;
            
            currentPicture.transform.SetParent(itemsContainer, true);
            currentPicture.transform.position = worldPosition;
            
            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                Debug.Log($"[PlacePicture] 배치 전 Alpha: {color.a}");
                color.a = 1f;
                sr.color = color;
                Debug.Log($"[PlacePicture] 배치 후 Alpha: {sr.color.a}");
            }

            PlacedPicture placed = currentPicture.AddComponent<PlacedPicture>();
            placed.Initialize(currentItem, this);
            
            Debug.Log($"[PlacePicture] {currentItem.itemName} 배치 완료!");

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
                InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
                if (inventory != null)
                    inventory.ReturnItem(currentItem);
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
            Destroy(picture);

            SpriteRenderer sr = currentPicture.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sortingOrder = 2000;

            MakeTransparent();
            isPlacing = true;
            placingFrameCount = 0;
            
            Debug.Log($"[StartMovingPicture] {currentItem.itemName} 이동 모드, 반투명 적용");
        }

        public void RemovePicture(PlacedPicture picture)
        {
            InventoryUI inventory = FindFirstObjectByType<InventoryUI>();
            if (inventory != null)
                inventory.ReturnItem(picture.item);
            
            Destroy(picture.gameObject);
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

        private void OnSaveLayout()
        {
            Debug.Log("[HousingSceneUI] 저장");
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
                Debug.Log($"[PlacedPicture] BoxCollider2D 추가됨!");
            }
            
            Debug.Log($"[PlacedPicture] {item.itemName} 초기화 완료, Collider: {collider != null}, Alpha: {spriteRenderer.color.a}");
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
            Debug.Log($"[PlacedPicture] ★★★ {item.itemName} 클릭됨! ★★★");
            housingUI.StartMovingPicture(this);
        }
        
        private void OnMouseEnter()
        {
            Debug.Log($"[PlacedPicture] 마우스 진입: {item.itemName}");
        }
    }
}