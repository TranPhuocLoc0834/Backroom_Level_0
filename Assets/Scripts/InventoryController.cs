using UnityEngine;
using StarterAssets;
using System.Collections;
using UnityEngine.InputSystem;

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private Coroutine hingeRoutine;

    [Header("Refs")]
    public FirstPersonController controller;
    public StarterAssetsInputs inputs;
    public PauseManager pauseManager;
    private bool isOpen = false;
    public bool IsOpen => isOpen;
    

    private void Start()
    {
        // reset hinge
        hingePivot.localEulerAngles = new Vector3(
            closeAngle,
            hingePivot.localEulerAngles.y,
            hingePivot.localEulerAngles.z
        );
    }

    void Update()
    {
        if (pauseManager != null && pauseManager.IsPaused)
            return;

        // StarterAssetsInputs sẽ tự set openInventory = true khi nhấn phím
        // if (inputs.openInventory)
        // {
        //     inputs.openInventory = false;
        //     ToggleInventory();
        // }
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
            
            Play(openSound);

            if (quickSlot != null)
                quickSlot.HideForInventory();

            StartHingeRotate(openAngle, null);

            inventoryPanel.SetActive(true);
            suitcaseRoot.SetActive(true);

            Time.timeScale = 0f;
            inputs.blockQuickInputs = true;
            controller.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Play(closeSound);

            StartHingeRotate(closeAngle, () =>
            {
                suitcaseRoot.SetActive(false);
                inventoryPanel.SetActive(false);

                if (quickSlot != null)        
                    quickSlot.ShowAfterInventory();              
            });

            Time.timeScale = 1f;
            inputs.blockQuickInputs = false;
            controller.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
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
