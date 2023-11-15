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
            RemoveItemUI(item);
        }
    }

    public void ToggleInventory()
    {
        bool isInventoryVisible = panel.activeSelf;
        ShowUI(!isInventoryVisible);
        PopulateList(playerInventory.InventoryItems, UseItem);
    }

    public override void ShowUI(bool show)
    {
        panel.SetActive(show);
        footerText.text = "";
        //if (show)
        //{
        //    GameStateManager.Instance.ChangeState(GameStateManager.GameState.Dialogue);
        //}
        //else
        //{
        //    GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        //}
    }
}
