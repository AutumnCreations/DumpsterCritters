using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSystem : MonoBehaviour
{
    public NPC shopKeeper;
    public PlayerInventory playerInventory;
    ShopUI shopUI;

    private void Awake()
    {
        shopUI = FindObjectOfType<ShopUI>();
        shopUI.gameObject.SetActive(false);
    }

    public void OpenShop()
    {
        shopUI.gameObject.SetActive(true);
        shopUI.PopulateShop(new List<ShopItem>(shopKeeper.GetItemsForSale()));
    }

    public void CloseShop()
    {
        GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        shopUI.gameObject.SetActive(false);
    }

    public void BuyItem(ShopItem item)
    {
        Debug.Log(item.item.name);
        if (playerInventory.GhostBucks >= item.cost)
        {
            playerInventory.SpendGhostBucks(item.cost);
            if (item.item.isFood)
            {
                playerInventory.FoodRations += item.item.rationCount;
            }
            else
            {
                playerInventory.AddItem(item.item);
            }
            shopUI.UpdateShopUI(item);
        }
        else
        {
            Debug.Log("Not enough Ghost Bucks");
        }
    }
}
