using System.Collections;
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

    public GameObject currentEquipped;  
    private GameObject currentConsumableFX;
    private Vector3 equipTargetPosition;
    public float moveSpeed = 5f; // tốc độ nhô/lặn
    private bool hiddenByInventory = false;
    private bool hiddenByFX = false;

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
    public void UseConsumable(Item item, System.Action onFXComplete = null)
    {
        if (item == null || item.type != ItemType.Consumable) return;

        HideForFX();

        // Spawn FX
        GameObject fx = null;
        if (item.prefab != null)
        {
            fx = Instantiate(item.prefab, handAnchor);
            fx.transform.localPosition = item.localPositionOffset;
            fx.transform.localRotation = Quaternion.Euler(item.localRotationOffset);
            currentConsumableFX = fx;

            ConsumableEffectRunner runner = fx.GetComponent<ConsumableEffectRunner>();
            if (runner == null) runner = fx.AddComponent<ConsumableEffectRunner>();

            runner.isHeal = (item.id == "003");
            runner.isStamina = (item.id == "004");

            // Coroutine chờ FX xong rồi restore
            StartCoroutine(WaitFXAndRestoreEquipped(fx, onFXComplete));
        }
        else
        {
            // Nếu không có prefab, gọi callback ngay
            onFXComplete?.Invoke();
            ShowAfterFX();
        }
    }

    private IEnumerator WaitFXAndRestoreEquipped(GameObject fx, System.Action onFXComplete)
    {
        while (fx != null)
            yield return null;

        ShowAfterFX();
        onFXComplete?.Invoke();
    }
    void UseQuickSlot(int index)
    {
        var slot = inventory.GetQuick(index);
        if (slot == null || slot.item == null || slot.IsEmpty)
        {
            UnequipItem();
            return;
        }

        Item item = slot.item;

        if (item.type == ItemType.Consumable)
        {
            // Gọi hàm chung
            UseConsumable(item);

            // Trừ item ngay
            slot.Remove(1);
            inventory.Notify();
            return;
        }

        // Non-consumable: logic cũ
        if (item.prefab)
        {
            if (currentEquipped && !currentEquipped.name.StartsWith(item.prefab.name))
            {
                equipTargetPosition = currentEquipped.transform.localPosition - Vector3.up * 3f;
                Destroy(currentEquipped);
                currentEquipped = null;
            }

            if (currentEquipped && currentEquipped.name.StartsWith(item.prefab.name))
                return;

            GameObject obj = Instantiate(item.prefab, handAnchor);
            obj.transform.localPosition = item.localPositionOffset - Vector3.up * 3f;
            obj.transform.localRotation = Quaternion.Euler(item.localRotationOffset);
            currentEquipped = obj;
            equipTargetPosition = item.localPositionOffset;
        }
    }

    public void OnInventoryOpen()
    {
        inputs.ResetQuickSlots();
    }

    public void HideForInventory()
    {
        if (currentEquipped && !hiddenByInventory)
        {
            currentEquipped.SetActive(false);
        }
            
        if (currentConsumableFX)
        {
            foreach (var r in currentConsumableFX.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }

    public void ShowAfterInventory()
    {
        if (currentEquipped && hiddenByInventory && !hiddenByFX)
        {
            currentEquipped.transform.localPosition = equipTargetPosition - Vector3.up * 3f;
            currentEquipped.SetActive(true);
        }
        if (currentConsumableFX)
        {
            foreach (var r in currentConsumableFX.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
        }
    }
    public void HideForFX()
    {
        if (currentEquipped && !hiddenByFX)
        {
            currentEquipped.SetActive(false);
            hiddenByFX = true;
        }
    }

    public void ShowAfterFX()
    {
        if (currentEquipped && hiddenByFX && !hiddenByInventory)
        {
            // đặt vị trí lặn trước
            currentEquipped.transform.localPosition = equipTargetPosition - Vector3.up * 3f;
            currentEquipped.SetActive(true);
            hiddenByFX = false;
        }
    }


    public void UnequipItem()
    {
        if (!currentEquipped) return;
       // đặt vị trí mục tiêu lặn xuống
        equipTargetPosition = currentEquipped.transform.localPosition - Vector3.up * 3f;

        // Lưu object tạm để destroy sau khi lặn
        GameObject toDestroy = currentEquipped;
        currentEquipped = null;

        // Coroutine chờ vài frame để lerp xong
        StartCoroutine(DestroyAfterLerp(toDestroy, 0.3f)); 
    }
    private IEnumerator DestroyAfterLerp(GameObject obj, float delay)
    {
        float timer = 0f;
        Vector3 startPos = obj.transform.localPosition;
        Vector3 endPos = startPos - Vector3.up * 3f;

        while (timer < delay)
        {
            timer += Time.unscaledDeltaTime; // dùng unscaled nếu timeScale = 0
            obj.transform.localPosition = Vector3.Lerp(startPos, endPos, timer / delay);
            yield return null;
        }

        Destroy(obj);
    }
}
