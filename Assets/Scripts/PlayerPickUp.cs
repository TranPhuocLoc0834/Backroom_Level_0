using UnityEngine;
using TMPro;
using StarterAssets; // ← namespace đây!

public class PlayerPickup : MonoBehaviour
{
    public Camera cam;
    public float range = 3f;
    public LayerMask mask;
    public Inventory inventory;

    public TextMeshProUGUI pickupText;
    public StarterAssetsInputs input;   // kéo player vào

    PickupItem current;

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, mask))
        {
            current = hit.collider.GetComponent<PickupItem>();

            if (current == null)
            {
                HidePickupUI();
                return;
            }

            ShowPickupUI($"[E] Pick up {current.item.displayName}");

            if (input.interact)
            {
                Debug.Log("Có nhấn E");
                TryPickup();
                input.interact = false; // reset để không spam
            }
        }
        else
        {
            current = null;
            HidePickupUI();
        }
    }

    void TryPickup()
    {
        if (current == null) return;
        Debug.Log("Có gọi hàm TryPickup");
        Pickup(current);
    }

    void Pickup(PickupItem p)
    {
        int leftover;
        
        if (p.item.type == ItemType.KeyItem)
        {
            Debug.Log("Có pickup");
            leftover = inventory.AddKeyItem(p.item, p.amount);
        }
        else
            leftover = inventory.AddItem(p.item, p.amount);

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
