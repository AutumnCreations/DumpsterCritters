using Unity.VisualScripting;
using UnityEngine;

public class InventorySystem : UIListManager<Interactable>
{
    protected override void Start()
    {
        base.Start();
        SubscribeToInventoryEvents();
    }

    private void SubscribeToInventoryEvents()
    {
        if (playerInventory != null)
        {
            playerInventory.OnItemAdded += HandleItemAdded;
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnItemAdded -= HandleItemAdded;
        }
    }

    private void HandleItemAdded(ItemData itemData)
    {
        Debug.Log(itemData.itemName + " added to inventory.");
        PopulateList(playerInventory.Inventory.GetItems(), UseItem);
    }

    private void UseItem(Item item)
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            bool equipped = player.EquipItem(item.itemData.itemPrefab);
            if (!equipped)
            {
                footerText.text = errorText;
                return;
            }
            footerText.text = "";

            item.quantity--;
            if (item.quantity > 0)
            {
                UpdateUI(item);
            }
            else
            {
                playerInventory.RemoveItem(item.itemData);
                RemoveItemUI(item);
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
