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

    private PickupItem current;

    void LateUpdate()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, mask))
        {
            current = hit.collider.GetComponent<PickupItem>();

            if (current == null)
            {
                HidePickupUI();
                input.interact = false;      // ← reset ở đây
                return;
            }

            ShowPickupUI($"[E] Pick up {current.item.displayName}");

            if (input.interact)
            {
                TryPickup();
                input.interact = false;      // ← reset khi dùng
            }
        }
        else
        {
            current = null;
            HidePickupUI();
            input.interact = false;          // ← reset khi không trúng
        }
    }

    void TryPickup()
    {
        if (current == null) return;
        Debug.Log("NHẬT LÚC NÀO: interact=" + input.interact);
        Pickup(current);
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
