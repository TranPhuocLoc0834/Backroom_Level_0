using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory;
    public InventorySlot slotPrefab;
    public Transform gridParent; // should contain 6x6 layout
    public Transform quickParent; // 1x6

    InventorySlot[,] uiSlots = new InventorySlot[Inventory.COLS, Inventory.ROWS];
    InventorySlot[] quickUi = new InventorySlot[Inventory.QUICK];

    void Start()
    {
        inventory.OnInventoryChanged += RefreshAll;
        BuildGrid();
        RefreshAll();
    }

    void BuildGrid()
    {
        // instantiate grid slots row-major
        for (int y = 0; y < Inventory.ROWS; y++)
        {
            for (int x = 0; x < Inventory.COLS; x++)
            {
                var go = Instantiate(slotPrefab, gridParent);
                go.gridX = x;
                go.gridY = y;
                uiSlots[x, y] = go;
            }
        }
        for (int i = 0; i < Inventory.QUICK; i++)
        {
            var go = Instantiate(slotPrefab, quickParent);
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

    public void OnSlotRightClick(InventorySlot slot)
    {
        // quick-use assign or consume
    }

    public void OnBeginDrag(InventorySlot slot, PointerEventData evt) { }

    public void OnDrag(InventorySlot slot, PointerEventData evt) { }

    public void OnEndDrag(InventorySlot slot, PointerEventData evt) { }

    public void OnDrop(InventorySlot slot, PointerEventData evt) { }
}
