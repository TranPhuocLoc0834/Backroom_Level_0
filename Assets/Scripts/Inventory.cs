// 3) Inventory.cs (core logic)
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int COLS = 8;
    public const int ROWS = 3; // main grid 6x6
    public const int QUICK = 5; // quick-use row

    // main grid flattened for easier serialization: size ROWS*COLS
    public ItemStack[] mainGrid = new ItemStack[ROWS * COLS];
    public ItemStack[] quickSlots = new ItemStack[QUICK];
    public event Action OnInventoryChanged;

    void Awake()
    {
        for (int i = 0; i < mainGrid.Length; i++)
            if (mainGrid[i] == null)
                mainGrid[i] = new ItemStack();
        for (int i = 0; i < quickSlots.Length; i++)
            if (quickSlots[i] == null)
                quickSlots[i] = new ItemStack();
    }

    public void Notify()
    {
        OnInventoryChanged?.Invoke();
    }

    int Index(int x, int y) => y * COLS + x;

    public ItemStack GetSlot(int x, int y)
    {
        if (x < 0 || x >= COLS || y < 0 || y >= ROWS)
            return null;
        return mainGrid[Index(x, y)];
    }

    public ItemStack GetQuick(int index)
    {
        if (index < 0 || index >= QUICK)
            return null;
        return quickSlots[index];
    }

    // Try to add item into inventory; returns leftover amount not added
    public int AddItem(Item item, int amount)
    {
        if (item == null || amount <= 0)
            return amount;

        switch (item.type)
        {
            case ItemType.KeyItem:
                return AddKeyItem(item, amount);

            case ItemType.Consumable:
                return AddConsumable(item, amount);

            default:
                // Những loại khác, ví dụ weapon, material → vào main grid
                return AddConsumable(item, amount);
        }
    }

    public int AddKeyItem(Item item, int amount)
    {
        if (item == null || amount <= 0)
            return amount;

        // Ignore non-key items
        if (item.type != ItemType.KeyItem)
            return amount;

        // Try merge into existing quick slots
        if (item.stackable)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                var s = quickSlots[i];
                if (s.item == item && s.amount < item.maxStack)
                {
                    amount = s.Add(amount);
                    if (amount <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return 0;
                    }
                }
            }
        }

        // Try place into empty quick slot
        for (int i = 0; i < quickSlots.Length && amount > 0; i++)
        {
            if (quickSlots[i].IsEmpty)
            {
                int toPlace = item.stackable ? Mathf.Min(item.maxStack, amount) : 1;
                quickSlots[i].item = item;
                quickSlots[i].amount = toPlace;
                amount -= toPlace;
                OnInventoryChanged?.Invoke(); // <--- BẮT BUỘC
            }
        }

        return amount; // leftover
    }

    public int AddConsumable(Item item, int amount)
    {
        if (item == null || amount <= 0)
            return amount;

        if (item.type != ItemType.Consumable)
            return amount;

        // Try merge
        if (item.stackable)
        {
            for (int i = 0; i < mainGrid.Length; i++)
            {
                var s = mainGrid[i];
                if (s.item == item && s.amount < item.maxStack)
                {
                    amount = s.Add(amount);
                    if (amount <= 0)
                        return 0;
                }
            }
        }

        // Try put into empty main slot
        for (int i = 0; i < mainGrid.Length && amount > 0; i++)
        {
            if (mainGrid[i].IsEmpty)
            {
                int toPlace = item.stackable ? Mathf.Min(item.maxStack, amount) : 1;
                mainGrid[i].item = item;
                mainGrid[i].amount = toPlace;
                amount -= toPlace;
            }
        }

        return amount;
    }

    // Move item between grid slots (handles swapping)
    public void MoveItemGrid(int fromX, int fromY, int toX, int toY)
    {
        if (fromX == toX && fromY == toY)
            return;
        var a = GetSlot(fromX, fromY);
        var b = GetSlot(toX, toY);
        if (a == null || b == null)
            return;

        // If same item and stackable try to merge
        if (!a.IsEmpty && !b.IsEmpty && a.item == b.item && a.item.stackable)
        {
            int leftover = b.Add(a.amount);
            a.amount = leftover;
            if (a.amount <= 0)
            {
                a.item = null;
                a.amount = 0;
            }
            return;
        }
        // swap
        var temp = a.Clone();
        a.item = b.item;
        a.amount = b.amount;
        b.item = temp.item;
        b.amount = temp.amount;
    }

    // Assign a main-grid slot to a quickslot (by reference copy)
    public void AssignToQuick(int sourceX, int sourceY, int quickIndex)
    {
        if (quickIndex < 0 || quickIndex >= QUICK)
            return;
        var src = GetSlot(sourceX, sourceY);
        if (src == null || src.IsEmpty)
            return;

        // copy content (not reference) so grid and quick slot can diverge
        quickSlots[quickIndex].item = src.item;
        quickSlots[quickIndex].amount = src.amount;
    }

    // Use quick slot
    // NHẤN 1–5 → Gọi hàm này
    public void UseQuickSlot(int index)
    {
        if (index < 0 || index >= QUICK)
            return;

        var s = quickSlots[index];
        if (s.IsEmpty) return;
        Item item = s.item;
        // Nếu consumable → giảm 1
        s.Remove(1);
        Notify();
    }
}
