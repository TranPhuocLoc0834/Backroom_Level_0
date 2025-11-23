using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    public GameObject inventoryPanel;

    private InputSystem_Actions actions;
    private bool isOpen = false;

    void Awake()
    {
        actions = new InputSystem_Actions();

        // Luôn lắng nghe nút mở/đóng Inventory
        actions.UI.OpenInventory.performed += ctx => ToggleInventory();
    }

    void OnEnable()
    {
        actions.UI.Enable();       // UI luôn bật
        actions.Player.Enable();   // Gameplay bật lên ban đầu
    }

    void OnDisable()
    {
        actions.UI.Disable();
        actions.Player.Disable();
    }

    void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            // khi mở inventory → khóa gameplay
            actions.Player.Disable();
        }
        else
        {
            // khi đóng → bật lại gameplay
            actions.Player.Enable();
        }
    }
}
