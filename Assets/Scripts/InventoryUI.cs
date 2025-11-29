using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public InventorySlot slotPrefab;
    public Transform gridParent; // should contain 6x6 layout
    public Transform quickParent; // 1x6
    private Canvas canvas;
    public PlayerInput inputs;

    [Header("Right Click Menu")]
    public GameObject rightClickMenu;
    public Button btnUse;
    public Button btnRemove;

     [Header("Consumable spawn settings")]
    public float defaultEffectTriggerPercent = 0.75f; // public cho bạn chỉnh
    public float defaultStaminaBuffDuration = 10f;
    public float defaultStaminaBuffMultiplier = 0.5f;
    public string defaultAnimName = ""; // optional override for runner.animName

    [Header("Inspect Settings")]
    public GameObject inspectPanel;         // Panel chế độ inspect
    public Image inspectIcon;               // Image hiển thị icon item
    public TextMeshProUGUI detailText;   


    private InventorySlot selectedSlot;   
    public QuickSlotController quickCtrl; 
    public InventoryController inventoryController;
    InventorySlot draggingSlot = null;
    Image dragIcon;
    ItemStack draggedStack;

    InventorySlot[,] uiSlots = new InventorySlot[Inventory.COLS, Inventory.ROWS];
    InventorySlot[] quickUi = new InventorySlot[Inventory.QUICK];
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    class PendingUse
    {
        public string itemId;
        public GameObject prefab; // may be null
    }
    private List<PendingUse> pendingUses = new List<PendingUse>();
    private bool wasOpen = false;
    private bool isProcessingQueue = false;
    void Start()
    {
        inventory.OnInventoryChanged += RefreshAll;
        BuildGrid();
        RefreshAll();

        // --- ADD ---
        // Create floating icon used during dragging
        var go = new GameObject("DragIcon", typeof(Image));
        dragIcon = go.GetComponent<Image>();
        dragIcon.raycastTarget = false;
        canvas = GetComponentInParent<Canvas>();
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.enabled = false;
        // --- FIX QUAN TRỌNG ---
        raycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        if (inventoryController != null)
            wasOpen = inventoryController.IsOpen;

    }

    void Update()
    {
         // Kiểm tra ESC để đóng inspect trước
        if (inspectPanel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseInspect();
            return; // tránh trigger Pause sau đó
        }

        if (rightClickMenu.activeSelf && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverContextMenu())
                rightClickMenu.SetActive(false);
        }
    //     // detect inventory close: wasOpen true -> now false => process pending uses
        if (inventoryController != null)
        {
            bool nowOpen = inventoryController.IsOpen;

            // detect close
            if (wasOpen && !nowOpen)
            {
                ProcessPendingUses();
                rightClickMenu.SetActive(false);
            }
                
            wasOpen = nowOpen;
        }
    }


    void BuildGrid()
    {
        // instantiate grid slots row-major
        for (int y = 0; y < Inventory.ROWS; y++)
        {
            for (int x = 0; x < Inventory.COLS; x++)
            {
                var go = Instantiate(slotPrefab, gridParent);
                go.isQuickSlot = false;
                go.gridX = x;
                go.gridY = y;
                uiSlots[x, y] = go;
            }
        }
        for (int i = 0; i < Inventory.QUICK; i++)
        {
            var go = Instantiate(slotPrefab, quickParent);
            go.isQuickSlot = true;
            go.quickIndex = i;
            quickUi[i] = go;
        }
    }

    public void RefreshAll()
    {
        for (int x = 0; x < Inventory.COLS; x++)
        for (int y = 0; y < Inventory.ROWS; y++)
        {
            uiSlots[x, y].Refresh(inventory.GetSlot(x, y));
        }
        for (int i = 0; i < Inventory.QUICK; i++)
        {
            quickUi[i].Refresh(inventory.GetQuick(i));
        }
    }

    // Event hooks from InventorySlot
    public void OnSlotLeftClick(InventorySlot slot)
    {
        // simple pick and place demo (no cursor stack shown here for brevity)
        // If you want full drag-and-drop with cursor-stack, add a static Dragging object
    }
    bool IsPointerOverContextMenu()
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == rightClickMenu ||
                r.gameObject.transform.IsChildOf(rightClickMenu.transform))
            {
                return true; // nằm trong menu
            }
        }

        return false; // nằm ngoài menu
    }


    public void OnSlotRightClick(InventorySlot slot)
    {
        selectedSlot = slot;
        // Lấy stack
        ItemStack s = slot.isQuickSlot
            ? inventory.GetQuick(slot.quickIndex)
            : inventory.GetSlot(slot.gridX, slot.gridY);

        if (s == null || s.IsEmpty)
            return;
        Debug.Log("Có gọi onRightClicklot");
        rightClickMenu.SetActive(true);
        btnUse.gameObject.SetActive(s.item.type == ItemType.Consumable);

        RectTransform menuRect = rightClickMenu.transform as RectTransform;
        RectTransform slotRect = slot.transform as RectTransform;

        // Chuyển vị trí slot → localPosition trong canvas
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, slotRect.position),
            canvas.worldCamera,
            out localPos
        );

        // Gán vị trí menu + offset
        menuRect.localPosition = localPos + new Vector2(120, -40);
    }
    public void OnClickUse()
    {
       if (selectedSlot == null) return;

        ItemStack s = selectedSlot.isQuickSlot
            ? inventory.GetQuick(selectedSlot.quickIndex)
            : inventory.GetSlot(selectedSlot.gridX, selectedSlot.gridY);

        if (s == null || s.IsEmpty) return;
        Item item = s.item;

        if (item.type == ItemType.Consumable && quickCtrl != null)
        {
            quickCtrl.UseConsumable(item, () =>
            {
                inventory.Notify();
                RefreshAll();
            });

            // Trừ item ngay
            s.Remove(1);
            if (s.amount <= 0) s.Clear();
            inventory.Notify();
            RefreshAll();

            rightClickMenu.SetActive(false);
        }
    }

    // Called when inventory closes (detected in Update)
    private void ProcessPendingUses()
    {
        if (isProcessingQueue) return;
        if (pendingUses == null || pendingUses.Count == 0) return;

        StartCoroutine(ProcessQueueRoutine());
    }

    private IEnumerator ProcessQueueRoutine()
    {
        isProcessingQueue = true;

        while (pendingUses.Count > 0)
        {
            PendingUse pu = pendingUses[0];
            pendingUses.RemoveAt(0);

            // Nếu có prefab tiêm
            if (pu.prefab != null && quickCtrl != null)
            {
                GameObject fx = Instantiate(pu.prefab, quickCtrl.handAnchor);
                ConsumableEffectRunner runner = fx.GetComponent<ConsumableEffectRunner>();
                if (runner == null) runner = fx.AddComponent<ConsumableEffectRunner>();

                // đưa config từ inspector của InventoryUI
                runner.effectTriggerPercent = defaultEffectTriggerPercent;
                runner.staminaBuffDuration = defaultStaminaBuffDuration;
                runner.staminaBuffMultiplier = defaultStaminaBuffMultiplier;
                if (!string.IsNullOrEmpty(defaultAnimName))
                    runner.animName = defaultAnimName;

                // loại hiệu ứng
                runner.isHeal = (pu.itemId == "003");
                runner.isStamina = (pu.itemId == "004");

                // CHỜ prefab chạy xong
                while (fx != null)
                    yield return null;
            }
            else
            {
                // Không có prefab → hiệu ứng ngay
                if (pu.itemId == "003")
                    PlayerStats.Instance?.Heal(PlayerStats.Instance.maxHealth);
                else if (pu.itemId == "004")
                {
                    PlayerStats.Instance?.RestoreStamina(PlayerStats.Instance.maxStamina);
                    PlayerStats.Instance?.ApplyStaminaBuff(defaultStaminaBuffDuration, defaultStaminaBuffMultiplier);
                }
            }

            // Món tiếp theo trong queue sẽ chạy sau món này
            yield return null;
        }

        isProcessingQueue = false;
    }

    public void OnClickDetail()
    {
        if (selectedSlot == null) return;

        ItemStack s = selectedSlot.isQuickSlot
            ? inventory.GetQuick(selectedSlot.quickIndex)
            : inventory.GetSlot(selectedSlot.gridX, selectedSlot.gridY);

        if (s == null || s.IsEmpty) return;

        Item item = s.item;

        // Bật panel
        inspectPanel.SetActive(true);

        if (inputs!=null)
            inputs.enabled = false;
        // Gán icon
        if (inspectIcon != null)
            inspectIcon.sprite = item.icon;

        // Gán mô tả
        if (detailText != null)
            detailText.text = item.description;

        rightClickMenu.SetActive(false);
    }

    public void CloseInspect()
    {
        inspectPanel.SetActive(false);
        if (inputs!=null)
            inputs.enabled = true;
        if (inspectIcon != null)
            inspectIcon.sprite = null; // reset icon
    }
    public void OnClickRemove()
    {
        if (selectedSlot == null) return;

        ItemStack s = selectedSlot.isQuickSlot
            ? inventory.GetQuick(selectedSlot.quickIndex)
            : inventory.GetSlot(selectedSlot.gridX, selectedSlot.gridY);

        if (s == null || s.IsEmpty) return;

        s.Remove(1);
        if (s.amount <= 0)
            s.Clear();

        inventory.Notify();
        RefreshAll();
        rightClickMenu.SetActive(false);
    }

    public void OnBeginDrag(InventorySlot slot, PointerEventData evt)
    {
        Debug.Log("Có gọi Drag");

        // Lấy stack nguồn đúng theo loại slot (quick hoặc grid)
        ItemStack s = slot.isQuickSlot
            ? inventory.GetQuick(slot.quickIndex)
            : inventory.GetSlot(slot.gridX, slot.gridY);

        if (s == null || s.IsEmpty) return;

        draggingSlot = slot;
        draggedStack = new ItemStack();
        draggedStack.item = s.item;
        draggedStack.amount = s.amount;

        dragIcon.sprite = draggedStack.item.icon;   
        dragIcon.enabled = true;

        // convert screen pos → canvas pos
        UpdateDragIconPosition(evt);
    }

    public void OnDrag(InventorySlot slot, PointerEventData evt)
    {
        if (dragIcon != null && dragIcon.enabled)
            UpdateDragIconPosition(evt);
    }

    private void UpdateDragIconPosition(PointerEventData evt)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            evt.position,
            canvas.worldCamera,
            out Vector2 localPos
        );
        dragIcon.rectTransform.localPosition = localPos;
    }

    public void OnEndDrag(InventorySlot slot, PointerEventData evt)
    {
        dragIcon.enabled = false;
        draggingSlot = null;
        draggedStack = null;
    }

    public void OnDrop(InventorySlot slot, PointerEventData evt)
    {
        if (draggingSlot == null || draggedStack == null) return;

        // Lấy ItemStack nguồn (theo nơi bắt đầu kéo)
        ItemStack src = draggingSlot.isQuickSlot
            ? inventory.GetQuick(draggingSlot.quickIndex)
            : inventory.GetSlot(draggingSlot.gridX, draggingSlot.gridY);

        // Lấy ItemStack đích (nơi thả)
        ItemStack dst = slot.isQuickSlot
            ? inventory.GetQuick(slot.quickIndex)
            : inventory.GetSlot(slot.gridX, slot.gridY);

        if (src == null || dst == null)
        {
            draggingSlot = null;
            draggedStack = null;
            dragIcon.enabled = false;
            return;
        }

        // Dùng hàm MoveStack đã có trong Inventory để xử lý move/merge/swap
        inventory.MoveStack(src, dst);

        // Cập nhật UI
        RefreshAll();

        // Dọn dẹp trạng thái kéo
        draggingSlot = null;
        draggedStack = null;
        dragIcon.enabled = false;
    }
}
