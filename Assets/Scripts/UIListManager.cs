using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public abstract class UIListManager<T> : MonoBehaviour
{
    [SerializeField]
    protected GameObject panel;
    [SerializeField]
    protected Transform itemListContainer;
    [SerializeField]
    protected GameObject itemUIPrefab;
    [SerializeField]
    protected PlayerInventory playerInventory;
    [SerializeField]
    protected TextMeshProUGUI footerText;
    [SerializeField]
    protected string errorText = "";

    protected Dictionary<Item, GameObject> itemUIElements = new Dictionary<Item, GameObject>();

    protected virtual void Start()
    {
        ShowUI(false);
    }

    protected void PopulateList(IEnumerable<Item> items, Action<Item> onItemAction)
    {
        // Clear existing UI elements
        foreach (Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }

        // Clear the itemUIElements dictionary
        itemUIElements.Clear();

        // Consolidate items into stacks
        var consolidatedItems = new Dictionary<string, Item>();
        foreach (var item in items)
        {
            if (consolidatedItems.ContainsKey(item.item.name))
            {
                consolidatedItems[item.item.name].quantity += item.quantity;
            }
            else
            {
                consolidatedItems.Add(item.item.name, new Item { item = item.item, quantity = item.quantity, cost = item.cost });
            }
        }

        // Populate UI with consolidated items
        foreach (var item in consolidatedItems.Values)
        {
            GameObject itemGO = Instantiate(itemUIPrefab, itemListContainer);
            SetupItemUI(itemGO, item);
            Button itemButton = itemGO.GetComponent<Button>();
            itemButton.onClick.AddListener(() => onItemAction(item));
        }
    }

    protected virtual void SetupItemUI(GameObject itemGO, Item item)
    {
        ItemUI itemUI = itemGO.GetComponent<ItemUI>();
        itemUI.itemNameText.text = item.item.itemName + " x" + item.quantity;
        itemUI.itemPriceText.text = $"{item.cost}";

        // Check if the item is already in the dictionary, if not, add it
        if (!itemUIElements.ContainsKey(item))
        {
            itemUIElements.Add(item, itemGO);
        }
    }


    //protected void PopulateList(List<Item> items, System.Action<Item> onItemAction)
    //{
    //    //foreach (Transform child in itemContainer)
    //    //{
    //    //    Destroy(child.gameObject);
    //    //}
    //    foreach (var item in items)
    //    {
    //        //Debug.Log(item);
    //        if (!itemUIElements.ContainsKey(item))
    //        {
    //            GameObject itemGO = Instantiate(itemUIPrefab, itemListContainer);
    //            SetupItemUI(itemGO, item);
    //            Button itemButton = itemGO.GetComponent<Button>();
    //            itemButton.onClick.AddListener(() => onItemAction(item));
    //        }
    //    }
    //}

    //protected virtual void SetupItemUI(GameObject itemGO, Item item)
    //{
    //    ItemUI itemUI = itemGO.GetComponent<ItemUI>();
    //    itemUI.itemNameText.text = item.item.itemName;
    //    itemUI.itemPriceText.text = $"{item.cost}";
    //    itemUI.itemImage.sprite = item.item.itemSprite;

    //    itemUIElements.Add(item, itemGO);
    //}
    protected virtual void UpdateUI(Item item, string newText = null)
    {
        if (itemUIElements.TryGetValue(item, out var itemGO))
        {
            ItemUI itemUI = itemGO.GetComponent<ItemUI>();
            Button itemButton = itemUI.GetComponent<Button>();
            itemButton.interactable = false;
            newText = newText ?? item.item.itemName + " x" + item.quantity;
            itemUI.itemNameText.text = newText;
        }
    }

    protected virtual void RemoveItemUI(Item item)
    {
        if (itemUIElements.TryGetValue(item, out var itemGO))
        {
            itemUIElements.Remove(item);
            Destroy(itemGO);
        }
    }

    public virtual void ShowUI(bool show)
    {
        panel.SetActive(show);
        footerText.text = "";
        if (show)
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.Paused);
        }
        else
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        }
    }

}
