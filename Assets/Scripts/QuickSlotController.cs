using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;   // dùng Starter Assets + New Input System

public class QuickSlotController : MonoBehaviour
{
    [Header("Reference")]
    public Inventory inventory;
    public Transform handAnchor;
    public StarterAssetsInputs inputs;

    [Header("Player Hand Mesh")]
    public GameObject handMesh;          // mesh bàn tay của nhân vật

    private GameObject currentEquipped;  

    void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();
    }
    // ===========================
    // PROCESS
    // ===========================
    void Update()
    {
        if (inputs.quick1) UseQuickSlot(0);
        if (inputs.quick2) UseQuickSlot(1);
        if (inputs.quick3) UseQuickSlot(2);
        if (inputs.quick4) UseQuickSlot(3);
        if (inputs.quick5) UseQuickSlot(4);

        // reset prevent spam
        inputs.quick1 = inputs.quick2 = inputs.quick3 = inputs.quick4 = inputs.quick5 = false;
    }

     void UseQuickSlot(int index)
    {
        Debug.Log("Có nhấn phím: " + index);
        var slot = inventory.GetQuick(index);
        if (slot.item == null || slot.IsEmpty)
        {
            Debug.Log("Bỏ trang bị");
            UnequipItem();
            return;
        }
            
        Item item = slot.item;

        if (currentEquipped)
            Destroy(currentEquipped);

        if (item.prefab)
        {
            GameObject obj = Instantiate(item.prefab, handAnchor); // spawn làm con của hand
            obj.transform.localPosition = item.localPositionOffset;
            obj.transform.localRotation = Quaternion.Euler(item.localRotationOffset);
            currentEquipped = obj;
            // tắt mesh tay nhân vật
            if (handMesh) handMesh.SetActive(false);
        }
        if (item.type == ItemType.Consumable)
        {
            slot.Remove(1);
            inventory.Notify();
        }
    }
    void UnequipItem()
    {
        if (currentEquipped)
            Destroy(currentEquipped);

        currentEquipped = null;

        // bật lại mesh tay
        if (handMesh) handMesh.SetActive(true);
    }
}

