using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<Item> items = new List<Item>();

    public void AddItem(ItemData newItem)
    {
        var existingItem = items.Find(i => i.itemData.itemName == newItem.itemName);
        if (existingItem != null)
        {
            existingItem.quantity += 1;
        }
        else
        {
            items.Add(new Item
            {
                itemData = newItem
            });
        }
    }

    public bool RemoveItem(ItemData item)
    {
        var existingItem = items.Find(i => i.itemData.itemName == item.itemName);
        if (existingItem != null)
        {
            existingItem.quantity -= 1;
            Debug.Log($"Removed {item.itemName} from inventory, {existingItem.quantity} remaining.");
            if (existingItem.quantity <= 0)
            {
                items.Remove(existingItem);
            }
            return true;
        }
        return false;
    }


    public List<Item> GetItems()
    {
        return items;
    }
}
