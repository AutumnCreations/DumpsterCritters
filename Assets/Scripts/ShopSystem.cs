using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : UIListManager<Item>
{
    //Temporary solution to NPC inventory issue
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
        if (playerInventory.GhostBucks >= item.cost)
        {
            playerInventory.SpendGhostBucks(item.cost);
            playerInventory.AddItem(item);
            npcInstance.GetItemsForSale().Remove(item);

            UpdateUI(item, "SOLD OUT");
            footerText.text = "";
        }
        else
        {
            footerText.text = errorText;
            //Debug.Log("Not enough Ghost Bucks");
        }
    }
}
