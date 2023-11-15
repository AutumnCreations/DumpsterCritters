using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<Item> items = new List<Item>();

    public void AddItem(Item newItem)
    {
        var existingItem = items.Find(i => i.item.itemName == newItem.item.itemName);
        if (existingItem != null)
        {
            existingItem.quantity += newItem.quantity;
        }
        else
        {
            items.Add(newItem);
        }
    }

    public bool RemoveItem(Item item)
    {
        var existingItem = items.Find(i => i.item.itemName == item.item.itemName);
        if (existingItem != null)
        {
            existingItem.quantity -= 1;
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
