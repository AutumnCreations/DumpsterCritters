using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    int foodRations;
    int ghostBucks;
    List<Interactable> inventoryItems = new List<Interactable>();

    public int FoodRations
    {
        get => foodRations;
        set => foodRations = Mathf.Max(0, value);
    }

    public int GhostBucks
    {
        get => ghostBucks;
        set => ghostBucks = Mathf.Max(0, value);
    }

    public List<Interactable> InventoryItems => new List<Interactable>(inventoryItems);

    public void AddItem(Interactable item)
    {
        if (item != null && !inventoryItems.Contains(item))
        {
            inventoryItems.Add(item);
        }
    }

    public bool RemoveItem(Interactable item)
    {
        return item != null && inventoryItems.Remove(item);
    }

    // Use this for consuming food rations
    public void ConsumeFood(int amount)
    {
        FoodRations -= amount;
    }

    // Use this for spending ghost bucks
    public void SpendGhostBucks(int amount)
    {
        GhostBucks -= amount;
    }
}
