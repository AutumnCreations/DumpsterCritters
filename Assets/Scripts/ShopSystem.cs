using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : UIListManager<Item>
{

    //Temporary solution to NPC inventory issue
    NPC npcInstance;
    private void Start()
    {
        npcInstance = FindObjectOfType<NPC>();
    }

    public void OpenShop(NPC npc)
    {
        ShowUI(true);
        PopulateList(npc.GetItemsForSale(), BuyItem);
    }

    public void CloseShop()
    {
        ShowUI(false);
    }

    public void BuyItem(Item item)
    {
        Debug.Log(item.item.name);
        if (playerInventory.GhostBucks >= item.cost)
        {
            playerInventory.SpendGhostBucks(item.cost);
            playerInventory.AddItem(item);
            npcInstance.GetItemsForSale().Remove(item);
            //if (item.item.isFood)
            //{
            //    playerInventory.FoodRations += item.item.rationCount;
            //}
            //else
            //{
            //    playerInventory.AddItem(item.item);
            //}

            UpdateUI(item, "SOLD OUT");
            
        }
        else
        {
            Debug.Log("Not enough Ghost Bucks");
        }
    }
}
