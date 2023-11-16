using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerInventory : MonoBehaviour
{
    //[AssetList(Path = "Assets/Interactables/Items")]
    [SerializeField]
    private Inventory inventory = new Inventory();
    [SerializeField]
    int foodRations;
    [SerializeField]
    int ghostBucks;
    [SerializeField]

    public Inventory Inventory => inventory;

    public delegate void ItemAddedHandler(ItemData itemData);

    public event ItemAddedHandler OnItemAdded;

    public void AddItem(ItemData itemData)
    {
        inventory.AddItem(itemData);
        OnItemAdded?.Invoke(itemData);
    }

    public bool RemoveItem(ItemData itemData) => inventory.RemoveItem(itemData);

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
