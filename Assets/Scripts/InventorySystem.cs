using UnityEngine;

public class InventorySystem : UIListManager<Interactable>
{
    private void UseItem(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {

            
        }
    }

    public void ToggleInventory()
    {
        bool isInventoryVisible = panel.activeSelf;
        ShowUI(!isInventoryVisible);
        PopulateList(playerInventory.InventoryItems, UseItem);
    }
}
