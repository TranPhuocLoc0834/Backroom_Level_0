using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;   // dùng Starter Assets + New Input System

public class QuickSlotController : MonoBehaviour
{
    [Header("Reference")]
    public Inventory inventory;
    public Transform handAnchor;
    public StarterAssetsInputs inputs;
    public InventoryController inventoryController;

    private GameObject currentEquipped;  
    private GameObject currentConsumableFX;
    private Vector3 equipTargetPosition;
    public float moveSpeed = 5f; // tốc độ nhô/lặn

    void Awake()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();
    }

    void Update()
    {
        if (Time.timeScale == 0f || (inventoryController != null && inventoryController.IsOpen)) return;        // đang pause
        if (inputs.quick1) UseQuickSlot(0);
        if (inputs.quick2) UseQuickSlot(1);
        if (inputs.quick3) UseQuickSlot(2);
        if (inputs.quick4) UseQuickSlot(3);
        if (inputs.quick5) UseQuickSlot(4);

        // reset prevent spam
        inputs.quick1 = inputs.quick2 = inputs.quick3 = inputs.quick4 = inputs.quick5 = false;

        // update vị trí nhô/lặn mượt
        if (currentEquipped)
        {
            currentEquipped.transform.localPosition = Vector3.Lerp(
                currentEquipped.transform.localPosition,
                equipTargetPosition,
                Time.deltaTime * moveSpeed
            );
        }
        // kiểm tra consumable xong
        if (currentConsumableFX == null && currentEquipped != null && !currentEquipped.activeSelf)
        {
            currentEquipped.SetActive(true);
            currentEquipped.transform.localPosition = equipTargetPosition;
        }
    }

    void UseQuickSlot(int index)
    {
        Debug.Log("Có nhấn phím: " + index);
        var slot = inventory.GetQuick(index);
        if (slot == null || slot.item == null || slot.IsEmpty)
        {
            Debug.Log("Bỏ trang bị / slot rỗng");
            UnequipItem();
            return;
        }

        Item item = slot.item;
       // Nếu đang cầm item non-consumable khác → nhô/lặn và destroy
        if (item.type != ItemType.Consumable)
        {
            // Nếu đang cầm item non-consumable khác → lặn + destroy
            if (currentEquipped && item.prefab && !currentEquipped.name.StartsWith(item.prefab.name))
            {
                equipTargetPosition = currentEquipped.transform.localPosition - Vector3.up * 3f;
                Destroy(currentEquipped);
                currentEquipped = null;
            }

            // Nếu đang cầm đúng item → không làm gì
            if (currentEquipped && currentEquipped.name.StartsWith(item.prefab.name))
                return;
        }

        // Consumable thì xử lý như trước
        if (item.type == ItemType.Consumable)
        {
            GameObject fx = null;
            if (item.prefab != null)
            {
                fx = Instantiate(item.prefab, handAnchor);
                fx.transform.localPosition = item.localPositionOffset;
                fx.transform.localRotation = Quaternion.Euler(item.localRotationOffset);
                currentConsumableFX = fx;

                ConsumableEffectRunner runner = fx.GetComponent<ConsumableEffectRunner>();
                if (runner == null) runner = fx.AddComponent<ConsumableEffectRunner>();

                if (item.id == "003")
                {
                    runner.isHeal = true;
                    runner.isStamina = false;
                }
                else if (item.id == "004")
                {
                    runner.isHeal = false;
                    runner.isStamina = true;
                }
            }
            // Ẩn currentEquipped tạm thời
            if (currentEquipped)
                currentEquipped.SetActive(false);
            
            slot.Remove(1);
            inventory.Notify();
            return;
        }

        // Non-consumable: nhô lên 3 đơn vị từ offset chuẩn
        if (item.prefab)
        {
            GameObject obj = Instantiate(item.prefab, handAnchor);
            obj.transform.localPosition = item.localPositionOffset - Vector3.up * 3f; // bắt đầu 3 đơn vị dưới offset
            obj.transform.localRotation = Quaternion.Euler(item.localRotationOffset);
            currentEquipped = obj;

            equipTargetPosition = item.localPositionOffset; // offset chuẩn là target
        }
    }

    public void OnInventoryOpen()
    {
        inputs.ResetQuickSlots();
    }

    public void HideForInventory()
    {
        if (currentEquipped)
            currentEquipped.SetActive(false);

        if (currentConsumableFX)
        {
            foreach (var r in currentConsumableFX.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }

    public void ShowAfterInventory()
    {
        if (currentEquipped)
            currentEquipped.SetActive(true);

        if (currentConsumableFX)
        {
            foreach (var r in currentConsumableFX.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
        }
    }

    public void UnequipItem()
    {
        Debug.Log("UNEQUIP CALLED");
        if (currentEquipped)
        {
            equipTargetPosition = currentEquipped.transform.localPosition - Vector3.up * 3f;
            Destroy(currentEquipped, 0.3f); // destroy sau khi lặn xuống
            currentEquipped = null;
        }
    }
}
