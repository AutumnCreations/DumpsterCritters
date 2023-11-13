using UnityEngine;

public class InventorySystem : UIListManager<Interactable>
{
    private void UseItem(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            bool equipped = player.EquipItem(item.item);
            if (!equipped) return;
            playerInventory.RemoveItem(item);
            RemoveItemUI(item);
        }
    }

    public void ToggleInventory()
    {
        bool isInventoryVisible = panel.activeSelf;
        ShowUI(!isInventoryVisible);
        PopulateList(playerInventory.InventoryItems, UseItem);
    }
}
