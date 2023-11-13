using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public GameObject itemEntryPrefab;
    public Transform itemEntryContainer;

    List<ItemUI> itemEntryGameObjects = new List<ItemUI>();
    ShopSystem shopSystem;

    public object item { get; private set; }

    private void Awake()
    {
        shopSystem = FindObjectOfType<ShopSystem>();
    }

    public void PopulateShop(List<Item> itemsForSale)
    {
        ClearShop();
        foreach (Item item in itemsForSale)
        {
            GameObject itemEntryGO = Instantiate(itemEntryPrefab, itemEntryContainer);
            ItemUI itemUI = itemEntryGO.GetComponent<ItemUI>();
            itemEntryGameObjects.Add(itemUI);
            itemUI.itemNameText.text = item.item.name;
            itemUI.itemPriceText.text = $"{item.cost}";
            //itemUI.itemImage.sprite = item.item.icon;

            Button purchaseButton = itemEntryGO.GetComponent<Button>();
            Debug.Log($"{purchaseButton} {itemEntryGO} {item.item}");
            purchaseButton.onClick.AddListener(() => shopSystem.BuyItem(item));
        }
    }

    public void UpdateShopUI(Item purchasedItem)
    {
        //Only designed to match by unique name for now
        ItemUI itemUI = itemEntryGameObjects.Find(
            item => item.itemNameText.text == purchasedItem.item.name);

        Button purchaseButton = itemUI.GetComponent<Button>();
        purchaseButton.interactable = false;
        itemUI.itemNameText.text = "SOLD OUT";
    }

    private void ClearShop()
    {
        foreach (ItemUI item in itemEntryGameObjects)
        {
            Destroy(item.gameObject);
        }
        itemEntryGameObjects.Clear();
    }

    public void ToggleShopVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
