using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : UIListManager<Item>
{
    NPC npcInstance;
    protected override void Start()
    {
        base.Start();
        npcInstance = FindObjectOfType<NPC>();
    }

    public void OpenShop(NPC npc)
    {
        ShowUI(true);
        PopulateList(npc.Inventory.GetItems(), BuyItem);
    }

    public void CloseShop()
    {
        ShowUI(false);
    }

    public void BuyItem(Item item)
    {
        //Debug.Log(item.item.name);
        if (playerInventory.GhostBucks >= item.itemData.cost)
        {
            playerInventory.SpendGhostBucks(item.itemData.cost);
            playerInventory.AddItem(item.itemData);
            npcInstance.Inventory.RemoveItem(item.itemData);
            footerText.text = "";

            if (item.quantity > 0)
            {
                UpdateUI(item);
            }
            else
            {
                UpdateUI(item, "SOLD OUT");
            }
        }
        else
        {
            footerText.text = errorText;
            //Debug.Log("Not enough Ghost Bucks");
        }
    }
}
