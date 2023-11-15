using UnityEngine;

public class InventorySystem : UIListManager<Interactable>
{
    private void UseItem(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            bool equipped = player.EquipItem(item.item);
            if (!equipped)
            {
                footerText.text = errorText;
                return;
            }
            footerText.text = "";
            playerInventory.RemoveItem(item);
            Debug.Log($"Player equipped {item.item.itemName}, {item.quantity} left in inventory.");
            if (item.quantity <= 0)
            {
                RemoveItemUI(item);
            }
            else
            {
                UpdateUI(item);
            }
        }
    }


    public void ToggleInventory()
    {
        bool isInventoryVisible = panel.activeSelf;
        ShowUI(!isInventoryVisible);
        PopulateList(playerInventory.Inventory.GetItems(), UseItem);
    }

    public override void ShowUI(bool show)
    {
        panel.SetActive(show);
        footerText.text = "";
    }
}
