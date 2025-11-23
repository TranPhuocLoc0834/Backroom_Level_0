// 4) InventorySlot.cs (UI element for a single slot)
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot
    : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
{
    public Image icon;
    public TextMeshProUGUI amountText;

    [HideInInspector]
    public int gridX,
        gridY;
    InventoryUI ui;

    void Awake()
    {
        ui = GetComponentInParent<InventoryUI>();
    }

    public void Refresh(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty)
        {
            icon.enabled = false;
            amountText.text = "";
            return;
        }
        icon.enabled = true;
        icon.sprite = stack.item.icon;
        amountText.text = stack.item.stackable ? stack.amount.ToString() : "";
    }

    // Simple click behaviour
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // TODO: pick up / place logic handled by InventoryUI
            ui.OnSlotLeftClick(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ui.OnSlotRightClick(this);
        }
    }

    // Drag handlers forward to UI manager (keeps slot lightweight)
    public void OnBeginDrag(PointerEventData eventData)
    {
        ui.OnBeginDrag(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui.OnDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ui.OnEndDrag(this, eventData);
    }

    public void OnDrop(PointerEventData eventData)
    {
        ui.OnDrop(this, eventData);
    }
}
