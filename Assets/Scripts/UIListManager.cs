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
            if (consolidatedItems.ContainsKey(item.itemData.itemName))
            {
                consolidatedItems[item.itemData.itemName].quantity += item.quantity;
            }
            else
            {
                consolidatedItems.Add(item.itemData.itemName, item);
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
        itemUI.itemNameText.text = item.itemData.itemName;
        itemUI.itemQuantityText.text = item.quantity.ToString();
        itemUI.itemPriceText.text = $"{item.itemData.cost}";
        itemUI.itemImage.sprite = item.itemData.itemSprite;
        Debug.Log(item.itemData.itemPrefab);
        itemUI.itemRationText.text = item.itemData.rationCount > 0 ? item.itemData.rationCount.ToString() : "";

        if (!itemUIElements.ContainsKey(item))
        {
            itemUIElements[item] = itemGO;
        }
        else
        {
            // Update existing UI with new quantity
            UpdateUI(item);
        }
    }

    protected virtual void UpdateUI(Item item, string newText = null)
    {
        if (itemUIElements.TryGetValue(item, out var itemGO))
        {
            ItemUI itemUI = itemGO.GetComponent<ItemUI>();
            newText = newText ?? item.itemData.itemName;
            itemUI.itemNameText.text = newText;
            itemUI.itemRationText.text = item.itemData.rationCount > 0 ? item.itemData.rationCount.ToString() : "";

            if (item.quantity > 0)
            {
                itemUI.itemQuantityText.text = item.quantity.ToString();
            }
            else
            {
                itemUI.itemQuantityText.text = "";
                Button itemButton = itemUI.GetComponent<Button>();
                itemButton.interactable = false;
            }
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
            if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Tutorial) 
                GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
        }
    }

}
