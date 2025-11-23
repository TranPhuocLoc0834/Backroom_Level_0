// 1) Item.cs (ScriptableObject)
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;
    public string displayName;
    public ItemType type;

    [Header("Offset")]
    public Vector3 localPositionOffset;
    public Vector3 localRotationOffset;

    [TextArea]
    public string description;
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 99;
    public GameObject prefab;
}

public enum ItemType
{
    Consumable,
    KeyItem,
}
