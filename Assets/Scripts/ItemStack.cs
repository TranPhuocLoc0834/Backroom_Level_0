// 2) ItemStack.cs
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public Item item;
    public int amount;

    public ItemStack()
    {
        item = null;
        amount = 0;
    }

    public ItemStack(Item i, int a)
    {
        item = i;
        amount = a;
    }

    public bool IsEmpty => item == null || amount <= 0;

    public int Add(int addAmount)
    {
        if (item == null)
            return addAmount; // nothing consumed
        if (!item.stackable)
            return addAmount; // can't stack

        int space = item.maxStack - amount;
        int toAdd = Mathf.Min(space, addAmount);
        amount += toAdd;
        return addAmount - toAdd; // leftover
    }

    public int Remove(int removeAmount)
    {
        if (item == null)
            return 0;
        int removed = Mathf.Min(removeAmount, amount);
        amount -= removed;
        if (amount <= 0)
        {
            item = null;
            amount = 0;
        }
        return removed;
    }

    public ItemStack Clone()
    {
        return new ItemStack(item, amount);
    }
}
