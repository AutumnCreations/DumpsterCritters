using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public GameObject itemEntryPrefab; 
    public Transform itemEntryContainer; 

    List<GameObject> itemEntryGameObjects = new List<GameObject>();
    ShopSystem shopSystem;

    private void Awake()
    {
        shopSystem = FindObjectOfType<ShopSystem>();
    }

    public void PopulateShop(List<ShopItem> itemsForSale)
    {
        ClearShop(); 
        foreach (ShopItem item in itemsForSale)
        {
            GameObject itemEntryGO = Instantiate(itemEntryPrefab, itemEntryContainer);
            itemEntryGameObjects.Add(itemEntryGO);
            //Cleanup these nasty string references
            ShopItemUI itemUI = itemEntryGO.GetComponent<ShopItemUI>();
            itemUI.itemNameText.text = item.item.name;
            itemUI.itemPriceText.text = $"{item.cost}";
            //itemUI.itemImage.sprite = item.item.icon;

            Button purchaseButton = itemEntryGO.GetComponent<Button>();
            Debug.Log($"{purchaseButton} {itemEntryGO} {item.item}");
            purchaseButton.onClick.AddListener(() => shopSystem.BuyItem(item));
        }
    }


    public void UpdateShopUI(ShopItem purchasedItem)
    {
        //foreach (GameObject itemEntryGO in itemEntryGameObjects)
        //{
        //    Text itemNameText = itemEntryGO.transform.Find("ItemName").GetComponent<Text>();
        //    Button purchaseButton = itemEntryGO.transform.Find("PurchaseButton").GetComponent<Button>();

        //    // If the item entry is the purchased item, update the UI accordingly
        //    if (itemNameText.text == purchasedItem.item.name)
        //    {
        //        purchaseButton.interactable = false;
        //                                            
        //        itemNameText.text = $"{purchasedItem.item.name} (Sold Out)";
        //        break;
        //    }
        //}
        Debug.Log("UpdateShopUI");
    }


    private void ClearShop()
    {
        foreach (GameObject itemEntryGO in itemEntryGameObjects)
        {
            Destroy(itemEntryGO);
        }
        itemEntryGameObjects.Clear();
    }

    public void ToggleShopVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
