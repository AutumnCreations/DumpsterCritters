using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField]
    private Inventory inventory = new Inventory();
    [SerializeField]
    int foodRations;
    [SerializeField]
    int ghostBucks;
    [SerializeField]
    //[AssetList(Path = "Assets/Interactables/Items")]
    List<Item> inventoryItems = new List<Item>();

    public Inventory Inventory => inventory;

    public void AddItem(Item item) => inventory.AddItem(item);
    public bool RemoveItem(Item item) => inventory.RemoveItem(item);

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

    //public List<Item> InventoryItems => new List<Item>(inventoryItems);

    //public void AddItem(Item item)
    //{
    //    if (item != null && !inventoryItems.Contains(item))
    //    {
    //        inventoryItems.Add(item);
    //    }
    //}

    //public bool RemoveItem(Item item)
    //{
    //    return item != null && inventoryItems.Remove(item);
    //}

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
