using UnityEngine;
using TMPro;
using StarterAssets;

public class PlayerPickup : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public LayerMask mask;
    public Inventory inventory;

    public TextMeshProUGUI pickupText;
    public StarterAssetsInputs input;

    private PickupItem currentItem;
    private NoteInteractable currentNote;

    void LateUpdate()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, mask))
        {
            // Ưu tiên pickupItem trước
            currentItem = hit.collider.GetComponent<PickupItem>();
            currentNote = hit.collider.GetComponent<NoteInteractable>();

            // ----------- Mảnh giấy ----------------
            if (currentNote != null)
            {
                ShowPickupUI("[E] Đọc mảnh giấy");

                if (input.interact)
                {
                    currentNote.Interact();
                    input.interact = false;
                }
                return;
            }

            // ----------- Item ---------------------
            if (currentItem != null)
            {
                ShowPickupUI($"[E] Pick up {currentItem.item.displayName}");

                if (input.interact)
                {
                    TryPickup();
                    input.interact = false;
                }
                return;
            }

            // Không phải note cũng không phải item
            HidePickupUI();
            input.interact = false;
        }
        else
        {
            currentItem = null;
            currentNote = null;
            HidePickupUI();
            input.interact = false;
        }
    }

    void TryPickup()
    {
        if (currentItem == null) return;
        Pickup(currentItem);
    }

    void Pickup(PickupItem p)
    {
        int leftover = inventory.AddItem(p.item, p.amount);

        if (leftover <= 0)
            Destroy(p.gameObject);
        else
            p.amount = leftover;
    }

    void ShowPickupUI(string text)
    {
        pickupText.gameObject.SetActive(true);
        pickupText.text = text;
    }

    void HidePickupUI()
    {
        pickupText.gameObject.SetActive(false);
    }
}
