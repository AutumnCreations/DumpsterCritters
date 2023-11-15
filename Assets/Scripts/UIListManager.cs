using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    protected void PopulateList(List<Item> items, System.Action<Item> onItemAction)
    {
        //foreach (Transform child in itemContainer)
        //{
        //    Destroy(child.gameObject);
        //}
        foreach (var item in items)
        {
            //Debug.Log(item);
            if (!itemUIElements.ContainsKey(item))
            {
                GameObject itemGO = Instantiate(itemUIPrefab, itemListContainer);
                SetupItemUI(itemGO, item);
                Button itemButton = itemGO.GetComponent<Button>();
                itemButton.onClick.AddListener(() => onItemAction(item));
            }
        }
    }

    protected virtual void SetupItemUI(GameObject itemGO, Item item)
    {
        ItemUI itemUI = itemGO.GetComponent<ItemUI>();
        itemUI.itemNameText.text = item.item.itemName;
        itemUI.itemPriceText.text = $"{item.cost}";
        itemUI.itemImage.sprite = item.item.itemSprite;

        itemUIElements.Add(item, itemGO);
    }
    protected virtual void UpdateUI(Item item, string newText)
    {
        if (itemUIElements.TryGetValue(item, out var itemGO))
        {
            //Can maybe add check for count here, and if food or not
            ItemUI itemUI = itemGO.GetComponent<ItemUI>();
            Button itemButton = itemUI.GetComponent<Button>();
            itemButton.interactable = false;
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
