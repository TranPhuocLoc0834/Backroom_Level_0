using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;

public class InventoryController : MonoBehaviour
{
    [Header("UI & Suitcase")]
    public GameObject inventoryPanel;
    public GameObject suitcaseRoot;
    public Transform hingePivot;
    public QuickSlotController quickSlot;

    [Header("Hinge Settings")]
    public float openAngle = -90f;
    public float closeAngle = 0f;
    public float hingeSpeed = 6f;

    private Coroutine hingeRoutine;

    [Header("Refs")]
    public FirstPersonController controller;
    public PauseManager pauseManager;

    private InputSystem_Actions input;
    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private void Awake()
    {
        // Không tạo PlayerInputs mới!
        input = new InputSystem_Actions();
        input.Player.OpenInventory.performed += _ => OpenInventory();
        // đảm bảo bản lề luôn ở trạng thái đóng từ đầu
        hingePivot.localEulerAngles = new Vector3(closeAngle, hingePivot.localEulerAngles.y, hingePivot.localEulerAngles.z);
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }
    public void DisableInventoryInput()
    {
        input.Player.OpenInventory.Disable();
    }

    public void EnableInventoryInput()
    {
        input.Player.OpenInventory.Enable();
    }

    public void OpenInventory()
    {
         if (pauseManager != null && pauseManager.IsPaused)
        {
            return;
        }
        quickSlot.inputs.ResetQuickSlots();
        ToggleInventory();   // luôn toggle
    }

    public void CloseInventory()
    {
        if (isOpen)
            ToggleInventory();
    }
    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            
            if (quickSlot != null)
                quickSlot.UnequipItem(); 
            // --- OPEN ---
            inventoryPanel.SetActive(true);
            suitcaseRoot.SetActive(true);
            Time.timeScale = 0f;
        
            if (controller) controller.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartHingeRotate(openAngle, null);
        }
        else
        {
            // --- CLOSE ---
            inventoryPanel.SetActive(false);

            Time.timeScale = 1f;
            if (controller) controller.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartHingeRotate(closeAngle, () =>
            {
                // Chỉ tắt khi xoay xong
                suitcaseRoot.SetActive(false);
            });
        }
    }

    private void StartHingeRotate(float target, System.Action onDone)
    {
        if (hingeRoutine != null)
            StopCoroutine(hingeRoutine);

        hingeRoutine = StartCoroutine(RotateHingeCoroutine(target, onDone));
    }

    private IEnumerator RotateHingeCoroutine(float target, System.Action onDone)
    {
        float start = hingePivot.localEulerAngles.x;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * hingeSpeed;
            float angle = Mathf.LerpAngle(start, target, t);

            hingePivot.localEulerAngles = new Vector3(
                angle,
                hingePivot.localEulerAngles.y,
                hingePivot.localEulerAngles.z
            );

            yield return null;
        }

        hingePivot.localEulerAngles = new Vector3(
            target,
            hingePivot.localEulerAngles.y,
            hingePivot.localEulerAngles.z
        );

        onDone?.Invoke();
    }
}
